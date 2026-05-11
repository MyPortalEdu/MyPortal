using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.VisibilityScopes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IBulletinRepository : IEntityRepository<Bulletin>
{
    Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken);

    Task<PageResult<BulletinSummaryResponse>> GetSummariesAsync(BulletinVisibilityScope scope,
        FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
}