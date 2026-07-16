using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the read-only Timetable area and the current user's own "my timetable" feed. Viewing
/// another staff member is gated under <see cref="StaffArea.Timetable"/> (self / line-manager / HR
/// view; editing the timetable is a central scheduling concern handled elsewhere). The "me" feed
/// is ungated — it's the caller's own home dashboard — and degrades gracefully: a user with no
/// linked person, or a person who isn't staff, sees only the public school diary (plus, for a
/// person, the events they're an attendee of). The absence source follows the same self/HR
/// confidentiality rule as the Absences area.
/// </summary>
public class StaffTimetableService : BaseService, IStaffTimetableService
{
    // A defensive cap so a malformed range can't ask the views to expand an unbounded window.
    private const int MaxWindowDays = 62;

    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IStaffCalendarRepository _calendarRepository;

    public StaffTimetableService(IAuthorizationService authorizationService, ILogger<StaffTimetableService> logger,
        IStaffMemberAccessService accessService, IStaffMemberRepository staffMemberRepository,
        IStaffCalendarRepository calendarRepository) : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _calendarRepository = calendarRepository;
    }

    public async Task<StaffCalendarResponse> GetCalendarAsync(Guid staffMemberId, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.Timetable,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        if (!TryNormaliseWindow(from, ref to))
        {
            return new StaffCalendarResponse();
        }

        // Absences ride along only if the viewer is entitled to the Absences area for this person.
        var canViewAbsences = await _accessService.CanAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);
        var includeConfidential = canViewAbsences &&
                                  await CanSeeConfidentialAbsencesAsync(staffMemberId, cancellationToken);

        var events = await GatherForStaffMemberAsync(staffMemberId, staffMember.PersonId, canViewAbsences,
            includeConfidential, from, to, cancellationToken);

        return Ordered(events);
    }

    public async Task<StaffCalendarResponse> GetMyCalendarAsync(DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        if (!TryNormaliseWindow(from, ref to))
        {
            return new StaffCalendarResponse();
        }

        var personId = AuthorizationService.GetCurrentUserPersonId();

        // No linked person (e.g. a service / system account) — only the public school diary applies.
        // GetDiaryEvents with an id that matches no attendee returns the public-only set.
        if (personId is null)
        {
            var publicOnly = await _calendarRepository.GetDiaryEventsAsync(Guid.Empty, from, to, cancellationToken);
            return Ordered(publicOnly.ToList());
        }

        var staffMemberId =
            await _staffMemberRepository.GetStaffMemberIdByPersonIdAsync(personId.Value, cancellationToken);

        // A person who isn't (active) staff: public events plus the ones they're an attendee of.
        if (staffMemberId is null)
        {
            var personEvents =
                await _calendarRepository.GetDiaryEventsAsync(personId.Value, from, to, cancellationToken);
            return Ordered(personEvents.ToList());
        }

        // The caller's own staff calendar — every source, confidential absences included (it's them).
        var events = await GatherForStaffMemberAsync(staffMemberId.Value, personId.Value, includeAbsences: true,
            includeConfidential: true, from, to, cancellationToken);

        return Ordered(events);
    }

    // Pulls every event source for a staff member into one list. Absences are optional (the caller
    // decides whether the viewer may see them and whether confidential rows are in scope).
    private async Task<List<StaffCalendarEventResponse>> GatherForStaffMemberAsync(Guid staffMemberId,
        Guid personId, bool includeAbsences, bool includeConfidential, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        var events = new List<StaffCalendarEventResponse>();

        events.AddRange(await _calendarRepository.GetLessonsAsync(staffMemberId, from, to, cancellationToken));
        events.AddRange(await _calendarRepository.GetDetentionsAsync(staffMemberId, from, to, cancellationToken));
        events.AddRange(await _calendarRepository.GetDiaryEventsAsync(personId, from, to, cancellationToken));
        events.AddRange(await _calendarRepository.GetNonContactAsync(staffMemberId, from, to, cancellationToken));
        events.AddRange(
            await _calendarRepository.GetParentEveningAppointmentsAsync(staffMemberId, from, to, cancellationToken));

        if (includeAbsences)
        {
            events.AddRange(await _calendarRepository.GetAbsencesAsync(staffMemberId, from, to, includeConfidential,
                cancellationToken));
        }

        return events;
    }

    private static StaffCalendarResponse Ordered(List<StaffCalendarEventResponse> events) => new()
    {
        Events = events.OrderBy(e => e.Start).ThenBy(e => e.End).ToList()
    };

    // Rejects an empty window and clamps an over-wide one (the calendar only ever asks for a
    // week/day/month, so the clamp only bites on a malformed request). Returns false if there's
    // nothing worth querying.
    private static bool TryNormaliseWindow(DateTime from, ref DateTime to)
    {
        if (to <= from)
        {
            return false;
        }

        if ((to - from).TotalDays > MaxWindowDays)
        {
            to = from.AddDays(MaxWindowDays);
        }

        return true;
    }

    // Mirrors the Absences area: HR (All scope) and the staff member themselves see confidential
    // rows; a line manager does not.
    private async Task<bool> CanSeeConfidentialAbsencesAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var relationship = await _accessService.GetRelationshipAsync(staffMemberId, cancellationToken);

        if (relationship == StaffRelationship.Self)
        {
            return true;
        }

        return await _accessService.CanAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewAll | StaffAccess.EditAll, cancellationToken);
    }
}
