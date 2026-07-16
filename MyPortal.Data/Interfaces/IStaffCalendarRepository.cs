using MyPortal.Contracts.Models.People;

namespace MyPortal.Data.Interfaces;

/// <summary>
/// Read-only calendar reads for a staff member's timetable. Each method projects one event
/// source straight into <see cref="StaffCalendarEventResponse"/>; the service concatenates them.
/// All windows are half-open overlap filters (<c>Start &lt; to AND End &gt; from</c>).
/// </summary>
public interface IStaffCalendarRepository
{
    /// <summary>Timetabled lessons the member teaches (cover-aware), plus reg sessions they tutor.</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetLessonsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken);

    /// <summary>Detentions the member supervises.</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetDetentionsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken);

    /// <summary>Public diary events, plus private events the member attends. Excludes kinds that
    /// have their own source (cover, detention, parent evening).</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetDiaryEventsAsync(Guid personId, DateTime from,
        DateTime to, CancellationToken cancellationToken);

    /// <summary>PPA / non-contact period allocations, expanded to dated instances.</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetNonContactAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken);

    /// <summary>Booked parent-evening appointments for the member.</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetParentEveningAppointmentsAsync(Guid staffMemberId,
        DateTime from, DateTime to, CancellationToken cancellationToken);

    /// <summary>The member's own absence/leave spans (all-day). Confidential rows only when
    /// <paramref name="includeConfidential"/>.</summary>
    Task<IEnumerable<StaffCalendarEventResponse>> GetAbsencesAsync(Guid staffMemberId, DateTime from,
        DateTime to, bool includeConfidential, CancellationToken cancellationToken);
}
