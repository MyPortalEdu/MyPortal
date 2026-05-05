using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class StudentGroupRepository : BaseEntityRepository<StudentGroup, Guid>, IStudentGroupRepository
{
    public StudentGroupRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task<IList<StudentGroup>> GetStudentGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_student_group_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<StudentGroup>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
