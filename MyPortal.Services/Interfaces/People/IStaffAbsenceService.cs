using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Absences &amp; Leave area: a staff member's absence/leave records. Gated under
/// <see cref="StaffArea.Absences"/> — self / line-manager / HR view, line-manager / HR edit (no
/// self-edit). Confidential absences are HR-and-self only; a line manager never sees or alters them.
/// </summary>
public interface IStaffAbsenceService
{
    Task<StaffAbsencesResponse> GetAbsencesAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task UpdateAbsencesAsync(Guid staffMemberId, StaffAbsencesUpsertRequest model,
        CancellationToken cancellationToken);
}
