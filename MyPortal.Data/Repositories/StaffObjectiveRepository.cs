using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffObjectiveRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffObjective>(factory, authorizationService), IStaffObjectiveRepository
{
    public async Task<IEnumerable<StaffObjective>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffObjective>(
                "[dbo].[usp_staff_objective_get_by_staff_member_id]", new { staffMemberId }, transaction,
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
