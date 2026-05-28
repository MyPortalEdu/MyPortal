using System.Data;
using Dapper;
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

        public async Task<PageResult<PersonSummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var sql = SqlResourceLoader.Load("People.GetPersonSummaries.sql");

            var result =
                await GetListPagedAsync<PersonSummaryResponse>(sql, null, filter, sort, paging, includeDeleted, cancellationToken);

            return result;
        }

        public async Task<PersonDetailsResponse?> GetDetailsByIdAsync(Guid personId, CancellationToken cancellationToken,
            IDbTransaction? transaction = null)
        {
            var (conn, owns) = AcquireConnection(transaction);

            try
            {
                var p = new { personId };

                var command = new CommandDefinition("[dbo].[usp_person_get_details_by_id]", p,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                
                await using var reader = await conn.QueryMultipleAsync(command);
                
                var header = await reader.ReadFirstOrDefaultAsync<PersonDetailsResponse>();

                if (header == null)
                {
                    return null;
                }
                
                // expand as required

                return header;
            }
            finally
            {
                if (owns)
                {
                    conn.Dispose();
                }
            }
        }
    }
}
