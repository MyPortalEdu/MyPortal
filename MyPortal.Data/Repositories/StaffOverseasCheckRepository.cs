using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffOverseasCheckRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffOverseasCheck>(factory, authorizationService), IStaffOverseasCheckRepository
{
    public async Task<IEnumerable<StaffOverseasCheck>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffOverseasCheck>(
                "[dbo].[usp_staff_overseas_check_get_by_staff_member_id]", new { staffMemberId }, transaction,
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
