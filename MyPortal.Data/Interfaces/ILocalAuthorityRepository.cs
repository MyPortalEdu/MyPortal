using MyPortal.Contracts.Models.School;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface ILocalAuthorityRepository : IEntityRepository<LocalAuthority>
{
    Task<PageResult<LocalAuthoritySummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
}
