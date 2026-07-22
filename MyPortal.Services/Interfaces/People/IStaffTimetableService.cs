using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffTimetableService
{
    /// <summary>
    /// The staff member's calendar entries overlapping [<paramref name="from"/>, <paramref name="to"/>):
    /// lessons, detentions, diary events, non-contact periods, parent-evening appointments and
    /// (when the viewer may see them) absences. Read-only; gated under <c>StaffArea.Timetable</c>.
    /// </summary>
    Task<StaffCalendarResponse> GetCalendarAsync(Guid staffMemberId, DateTime from, DateTime to,
        CancellationToken cancellationToken);

    /// <summary>
    /// The current user's own calendar for the window — ungated (their own dashboard). A user with
    /// no linked person sees only the public school diary; a person who isn't staff additionally
    /// sees the events they attend; an active staff member gets their full timetable.
    /// </summary>
    Task<StaffCalendarResponse> GetMyCalendarAsync(DateTime from, DateTime to,
        CancellationToken cancellationToken);
}
