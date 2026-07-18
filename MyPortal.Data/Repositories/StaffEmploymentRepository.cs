using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffEmploymentRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffEmployment>(factory, authorizationService), IStaffEmploymentRepository
{
    public async Task<IEnumerable<StaffEmployment>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffEmployment>(
                "[dbo].[usp_staff_employment_get_by_staff_member_id]", new { staffMemberId }, transaction,
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
