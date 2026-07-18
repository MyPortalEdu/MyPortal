using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class PayScalePointRateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PayScalePointRate>(factory, authorizationService), IPayScalePointRateRepository
{
    public async Task<IEnumerable<PayScalePointRate>> GetCurrentByZoneAsync(Guid payZoneId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<PayScalePointRate>(
                "[dbo].[usp_pay_scale_point_rate_get_current_by_zone]", new { payZoneId, asOf }, transaction,
                cancellationToken: cancellationToken);
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
