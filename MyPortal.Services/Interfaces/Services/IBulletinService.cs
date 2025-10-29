using MyPortal.Contracts.Models.Bulletins;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

public interface IBulletinService
{
    Task<BulletinDetailsDto?> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken);
    Task<PageResult<BulletinSummaryDto>> GetBulletinsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateBulletinAsync(BulletinUpsertDto model, CancellationToken cancellationToken);
    Task UpdateBulletinAsync(Guid bulletinId, BulletinUpsertDto model, CancellationToken cancellationToken);
    Task DeleteBulletinAsync(Guid bulletinId, CancellationToken cancellationToken);
    Task UpdateBulletinApprovalAsync(Guid bulletinId, bool isApproved, CancellationToken cancellationToken);
}