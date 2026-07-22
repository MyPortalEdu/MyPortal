using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffPerformanceService
{
    Task<StaffPerformanceResponse> GetPerformanceAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task UpdatePerformanceAsync(Guid staffMemberId, StaffPerformanceUpsertRequest model,
        CancellationToken cancellationToken);
}
