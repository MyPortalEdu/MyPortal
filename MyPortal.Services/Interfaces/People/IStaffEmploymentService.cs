using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Employment Details area: bank / NI details plus the staff member's employment spells and
/// their contracts. Access is gated under <see cref="StaffArea.EmploymentDetails"/> — self / HR
/// view, HR-only edit (no line-manager scope).
/// </summary>
public interface IStaffEmploymentService
{
    Task<StaffEmploymentDetailsResponse> GetEmploymentDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateEmploymentDetailsAsync(Guid staffMemberId, StaffEmploymentDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
