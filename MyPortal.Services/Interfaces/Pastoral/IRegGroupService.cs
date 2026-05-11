using MyPortal.Contracts.Models.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Pastoral;

public interface IRegGroupService
{
    Task<PageResult<RegGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
    Task<RegGroupDetailsResponse> GetDetailsByIdAsync(Guid regGroupId,
        CancellationToken cancellationToken);

    Task<Guid> CreateAsync(RegGroupUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateAsync(Guid regGroupId, RegGroupUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteAsync(Guid regGroupId, CancellationToken cancellationToken);
}