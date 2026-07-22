using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffNextOfKinService
{
    Task<StaffNextOfKinAreaResponse> GetNextOfKinAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task UpdateNextOfKinAsync(Guid staffMemberId, StaffNextOfKinAreaUpsertRequest model,
        CancellationToken cancellationToken);
}
