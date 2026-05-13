using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.School;

public interface ISchoolService
{
    Task<PageResult<SchoolDetailsResponse>> GetListPagedAsync(FilterOptions? filter, SortOptions? sort, int page,
        int pageSize, CancellationToken cancellationToken);

    Task<SchoolDetailsResponse?> GetLocalSchoolDetailsAsync(CancellationToken cancellationToken);

    Task<SchoolDetailsResponse?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateAsync(SchoolUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null);

    Task UpdateAsync(Guid id, SchoolUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null);

    Task<Guid> CreateOrUpdateLocalSchoolAsync(SchoolUpsertRequest model, CancellationToken cancellationToken);
}
