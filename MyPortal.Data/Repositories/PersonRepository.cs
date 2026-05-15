using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
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

        public async Task<PageResult<PersonSummaryResponse>> GetPeople(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var sql = SqlResourceLoader.Load("People.GetPersonSummaries.sql");

            var result =
                await GetListPagedAsync<PersonSummaryResponse>(sql, null, filter, sort, paging, includeDeleted, cancellationToken);

            return result;
        }

        public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default)
        {
            var sql = SqlResourceLoader.Load("People.GetStaffMemberSummaries.sql");

            return await GetListPagedAsync<StaffMemberSummaryResponse>(sql, null, filter, sort, paging, false,
                cancellationToken);
        }
    }
}
