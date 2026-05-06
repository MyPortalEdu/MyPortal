using MyPortal.Contracts.Models.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Pastoral;

public interface IHouseService
{
    Task<PageResult<HouseSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
    Task<HouseDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateHouseAsync(HouseUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateHouseAsync(Guid id, HouseUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteHouseAsync(Guid id, CancellationToken cancellationToken);
}