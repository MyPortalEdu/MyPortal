using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffQualificationRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffQualification>(factory, authorizationService), IStaffQualificationRepository
{
    public async Task<IEnumerable<StaffQualification>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffQualification>(
                "[dbo].[usp_staff_qualification_get_by_staff_member_id]", new { staffMemberId }, transaction,
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
