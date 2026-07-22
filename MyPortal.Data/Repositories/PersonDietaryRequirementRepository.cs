using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class PersonDietaryRequirementRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PersonDietaryRequirement>(factory, authorizationService), IPersonDietaryRequirementRepository
{
    public async Task<IEnumerable<PersonDietaryRequirement>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<PersonDietaryRequirement>(
                "[dbo].[usp_person_dietary_requirement_get_by_person_id]", new { personId }, transaction,
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
