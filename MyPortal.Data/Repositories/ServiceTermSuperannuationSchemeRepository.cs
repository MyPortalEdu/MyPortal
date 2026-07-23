using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class ServiceTermSuperannuationSchemeRepository(
    IDbConnectionFactory factory,
    IAuthorizationService authorizationService)
    : EntityRepository<ServiceTermSuperannuationScheme>(factory, authorizationService),
        IServiceTermSuperannuationSchemeRepository
{
    public async Task<IEnumerable<ServiceTermSuperannuationScheme>> GetByServiceTermIdsAsync(
        IEnumerable<Guid> serviceTermIds, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = serviceTermIds.ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        const string sql =
            "SELECT [Id], [ServiceTermId], [SuperannuationSchemeId], [IsMain] " +
            "FROM [dbo].[ServiceTermSuperannuationSchemes] WHERE [ServiceTermId] IN @ids;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { ids }, transaction,
                cancellationToken: cancellationToken);
            return await conn.QueryAsync<ServiceTermSuperannuationScheme>(command);
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
