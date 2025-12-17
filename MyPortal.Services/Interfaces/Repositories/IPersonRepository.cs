using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IPersonRepository : IEntityRepository<Person>
    {
        Task<PageResult<PersonSummaryResponse>> GetPeople(FilterOptions? filter = null, SortOptions? sort = null,
            PageOptions? paging = null, bool includeDeleted = false,
            CancellationToken cancellationToken = default);
    }
}
