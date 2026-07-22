using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Pre-Employment Checks (Single Central Record) area: the summary SCR flags plus the DBS,
/// right-to-work, reference and overseas-check lists. Access is gated under
/// <see cref="StaffArea.PreEmploymentChecks"/> — safeguarding/HR data, All-scope view and edit
/// only (no self or line-manager scope).
/// </summary>
public interface IStaffPreEmploymentService
{
    Task<StaffPreEmploymentChecksResponse> GetPreEmploymentChecksAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdatePreEmploymentChecksAsync(Guid staffMemberId, StaffPreEmploymentChecksUpsertRequest model,
        CancellationToken cancellationToken);
}
