using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentGroupSupervisorRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StudentGroupSupervisor>(factory, authorizationService), IStudentGroupSupervisorRepository
{
    public async Task<IList<StudentGroupSupervisor>> GetStudentGroupSupervisorsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_student_group_supervisor_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<StudentGroupSupervisor>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task<IList<StudentGroupSupervisor>> GetByStudentGroupAsync(Guid studentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<StudentGroupSupervisor>(
                "[dbo].[usp_student_group_supervisor_get_by_student_group_id]",
                new { studentGroupId }, transaction, cancellationToken: cancellationToken);

            return result.ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
