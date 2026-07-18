using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffPreEmploymentChecksRepository(
    IDbConnectionFactory factory,
    IAuthorizationService authorizationService)
    : EntityRepository<StaffPreEmploymentChecks>(factory, authorizationService),
        IStaffPreEmploymentChecksRepository
{
    public async Task<StaffPreEmploymentChecks?> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<StaffPreEmploymentChecks>(
                "[dbo].[usp_staff_pre_employment_checks_get_by_staff_member_id]", new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return rows.FirstOrDefault();
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
