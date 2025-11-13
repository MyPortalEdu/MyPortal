using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories
{
    public class PersonRepository : EntityRepository<Person>, IPersonRepository
    {
        public PersonRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
            factory, authorizationService)
        {
        }

        public async Task<PageResult<PersonSummaryDto>> GetPeople(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var sql = SqlResourceLoader.Load("People.GetPersonSummaries.sql");

            var result =
                await GetListPagedAsync<PersonSummaryDto>(sql, null, filter, sort, paging, includeDeleted, cancellationToken);

            return result;
        }
    }
}
