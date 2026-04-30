using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using MyPortal.Services.School.Bulletins;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IBulletinRepository : IEntityRepository<Bulletin>
{
    Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken);

    Task<PageResult<BulletinSummaryResponse>> GetSummariesAsync(BulletinVisibilityScope scope,
        FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
}