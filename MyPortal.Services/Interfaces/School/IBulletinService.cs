using MyPortal.Contracts.Models.Bulletins;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.School;

public interface IBulletinService
{
    Task<BulletinDetailsResponse> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken);
    Task<PageResult<BulletinSummaryResponse>> GetBulletinSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateBulletinAsync(BulletinUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateBulletinAsync(Guid bulletinId, BulletinUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteBulletinAsync(Guid bulletinId, CancellationToken cancellationToken);
    Task UpdateBulletinApprovalAsync(Guid bulletinId, bool isApproved, long expectedVersion,
        CancellationToken cancellationToken);
}