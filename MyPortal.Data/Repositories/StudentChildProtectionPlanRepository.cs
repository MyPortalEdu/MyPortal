using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentChildProtectionPlanRepository(
    IDbConnectionFactory factory,
    IAuthorizationService authorizationService)
    : EntityRepository<StudentChildProtectionPlan>(factory, authorizationService),
        IStudentChildProtectionPlanRepository
{
    public async Task<IEnumerable<StudentChildProtectionPlan>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StudentChildProtectionPlan>(
                "[dbo].[usp_student_child_protection_plan_get_by_student_id]", new { studentId }, transaction,
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
