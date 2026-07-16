using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Professional Details area: teaching status, QTS / induction, and the staff member's
/// structured qualifications. Access is relationship-scoped under
/// <see cref="StaffArea.ProfessionalDetails"/> (Own / Managed / All view; HR or line-manager
/// edit — no self-edit, as the data is HR-verified).
/// </summary>
public interface IStaffProfessionalService
{
    Task<StaffProfessionalDetailsResponse> GetProfessionalDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateProfessionalDetailsAsync(Guid staffMemberId, StaffProfessionalDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
