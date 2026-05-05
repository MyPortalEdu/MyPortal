using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class StudentGroupSupervisorRepository : BaseEntityRepository<StudentGroupSupervisor, Guid>, IStudentGroupSupervisorRepository
{
    public StudentGroupSupervisorRepository(IConnectionFactory factory) : base(factory)
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
