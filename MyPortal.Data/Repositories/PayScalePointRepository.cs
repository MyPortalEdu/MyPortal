using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class PayScalePointRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PayScalePoint>(factory, authorizationService), IPayScalePointRepository
{
    public async Task<IEnumerable<PayScaleUsageRow>> GetContractCountsAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT P.[Id], " +
            "(SELECT COUNT(*) FROM [dbo].[StaffContracts] SC " +
            "WHERE SC.[PayScalePointId] = P.[Id] AND SC.[IsDeleted] = 0) AS [ContractCount] " +
            "FROM [dbo].[PayScalePoints] P;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, null, transaction, cancellationToken: cancellationToken);
            return await conn.QueryAsync<PayScaleUsageRow>(command);
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
