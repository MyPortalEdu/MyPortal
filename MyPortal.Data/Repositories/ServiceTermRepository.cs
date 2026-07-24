using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class ServiceTermRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<ServiceTerm>(factory, authorizationService), IServiceTermRepository
{
    public async Task<IEnumerable<ServiceTermRow>> GetAllWithUsageAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT ST.[Id], ST.[Code], ST.[Description], ST.[Active], ST.[IsTeacher], ST.[Salaried], " +
            "ST.[SpinalProgression], ST.[SinglePaySpine], ST.[TermTimeOnlyPossible], ST.[IncrementMonth], " +
            "ST.[IncrementDay], ST.[MinimumPoint], ST.[MaximumPoint], ST.[PointInterval], " +
            "ST.[HoursPerWeek], ST.[WeeksPerYear], " +
            "(SELECT COUNT(*) FROM [dbo].[StaffContracts] SC " +
            "WHERE SC.[ServiceTermId] = ST.[Id] AND SC.[IsDeleted] = 0) AS [ContractCount], " +
            "(SELECT COUNT(*) FROM [dbo].[Posts] P " +
            "WHERE P.[ServiceTermId] = ST.[Id] AND P.[IsDeleted] = 0) AS [PostCount] " +
            "FROM [dbo].[ServiceTerms] ST ORDER BY ST.[Code];";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, null, transaction, cancellationToken: cancellationToken);
            return await conn.QueryAsync<ServiceTermRow>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeServiceTermId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM [dbo].[ServiceTerms] " +
            "WHERE [Code] = @code AND (@excludeServiceTermId IS NULL OR [Id] <> @excludeServiceTermId)) " +
            "THEN 1 ELSE 0 END AS bit);";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { code, excludeServiceTermId }, transaction,
                cancellationToken: cancellationToken);
            return await conn.ExecuteScalarAsync<bool>(command);
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
