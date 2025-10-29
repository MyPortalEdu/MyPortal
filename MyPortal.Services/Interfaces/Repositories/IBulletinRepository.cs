using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IBulletinRepository : IEntityRepository<Bulletin>
{
    Task<BulletinDetailsDto?> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken);
    
    Task<PageResult<BulletinSummaryDto>> GetBulletinsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
}