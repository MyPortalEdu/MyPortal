using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class SuperannuationSchemeRateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<SuperannuationSchemeRate>(factory, authorizationService), ISuperannuationSchemeRateRepository
{
    public async Task<IEnumerable<SuperannuationSchemeRate>> GetCurrentAsync(DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<SuperannuationSchemeRate>(
                "[dbo].[usp_superannuation_scheme_rate_get_current]", new { asOf }, transaction,
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
