using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffResponsibilityService
{
    Task<StaffResponsibilitiesResponse> GetResponsibilitiesAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateResponsibilitiesAsync(Guid staffMemberId, StaffResponsibilitiesUpsertRequest model,
        CancellationToken cancellationToken);
}
