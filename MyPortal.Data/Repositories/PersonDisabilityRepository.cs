using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class PersonDisabilityRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PersonDisability>(factory, authorizationService), IPersonDisabilityRepository
{
    public async Task<IEnumerable<PersonDisability>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<PersonDisability>(
                "[dbo].[usp_person_disability_get_by_person_id]", new { personId }, transaction,
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
