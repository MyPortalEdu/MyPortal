using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class PayScalePointRateRepository : EntityRepository<PayScalePointRate>, IPayScalePointRateRepository
{
    public PayScalePointRateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }

    public async Task<IEnumerable<PayScalePointRate>> GetCurrentByZoneAsync(Guid payZoneId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT [Id], [PayScalePointId], [PayZoneId], [EffectiveFrom], [EffectiveTo], [AnnualSalary] " +
            "FROM [dbo].[PayScalePointRates] " +
            "WHERE [PayZoneId] = @payZoneId AND [EffectiveFrom] <= @asOf " +
            "AND ([EffectiveTo] IS NULL OR [EffectiveTo] >= @asOf);";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { payZoneId, asOf }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<PayScalePointRate>(command);
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
