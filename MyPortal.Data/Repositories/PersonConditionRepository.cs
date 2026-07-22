using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class PersonConditionRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PersonCondition>(factory, authorizationService), IPersonConditionRepository
{
    public async Task<IEnumerable<PersonCondition>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<PersonCondition>(
                "[dbo].[usp_person_condition_get_by_person_id]", new { personId }, transaction,
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
