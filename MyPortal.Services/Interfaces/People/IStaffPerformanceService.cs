using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffPerformanceService
{
    Task<StaffPerformanceResponse> GetPerformanceAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task UpdatePerformanceAsync(Guid staffMemberId, StaffPerformanceUpsertRequest model,
        CancellationToken cancellationToken);
}
