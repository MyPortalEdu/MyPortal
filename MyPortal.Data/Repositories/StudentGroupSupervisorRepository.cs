using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentGroupSupervisorRepository : EntityRepository<StudentGroupSupervisor>, IStudentGroupSupervisorRepository
{
    public StudentGroupSupervisorRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<StudentGroupSupervisor>> GetStudentGroupSupervisorsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_student_group_supervisor_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<StudentGroupSupervisor>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
