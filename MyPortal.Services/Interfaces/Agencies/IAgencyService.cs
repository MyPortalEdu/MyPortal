using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Agencies;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Agencies;

public interface IAgencyService
{
    Task<PageResult<AgencySummaryResponse>> GetListPagedAsync(FilterOptions? filter, SortOptions? sort,
        int page, int pageSize, CancellationToken cancellationToken);
    Task<AgencyDetailsResponse> GetDetailsByIdAsync(int id, CancellationToken cancellationToken);

    Task<Guid> CreateAsync(AgencyUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null);

    Task UpdateAsync(Guid agencyId, AgencyUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null);
}