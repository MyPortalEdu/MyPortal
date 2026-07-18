using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;

namespace MyPortal.Data.Repositories
{
    public class PersonRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : EntityRepository<Person>(factory, authorizationService), IPersonRepository
    {
        public async Task<IReadOnlyList<PersonSearchResponse>> SearchAsync(string like, CancellationToken cancellationToken)
        {
            var sql = SqlResourceLoader.Load("People.SearchPeople.sql");

            using var conn = _factory.Create();

            var command = new CommandDefinition(sql, new { like }, cancellationToken: cancellationToken);
            var rows = await conn.QueryAsync<PersonSearchResponse>(command);

            return rows.AsList();
        }
    }
}
