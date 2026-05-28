using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IPersonRepository : IEntityRepository<Person>
{
    Task<PageResult<PersonSummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<PersonDetailsResponse?> GetDetailsByIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}