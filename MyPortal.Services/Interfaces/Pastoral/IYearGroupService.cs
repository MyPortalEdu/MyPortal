using MyPortal.Contracts.Models.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Pastoral;

public interface IYearGroupService
{
    Task<PageResult<YearGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
    Task<YearGroupDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(YearGroupUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, YearGroupUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
