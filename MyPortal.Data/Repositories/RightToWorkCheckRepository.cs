using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class RightToWorkCheckRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<RightToWorkCheck>(factory, authorizationService), IRightToWorkCheckRepository
{
    public async Task<IEnumerable<RightToWorkCheck>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<RightToWorkCheck>(
                "[dbo].[usp_right_to_work_check_get_by_staff_member_id]", new { staffMemberId }, transaction,
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
