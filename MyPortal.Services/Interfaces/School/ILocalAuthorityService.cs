using MyPortal.Contracts.Models.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.School;

public interface ILocalAuthorityService
{
    /// <summary>
    /// Paged summary of local authorities for the LA picker on the school
    /// details page (and anywhere else a "pick an LA" UI is needed).
    /// </summary>
    Task<PageResult<LocalAuthoritySummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
}
