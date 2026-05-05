using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class AttendancePeriodRepository : BaseEntityRepository<AttendancePeriod, Guid>, IAttendancePeriodRepository
{
    public AttendancePeriodRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task<IList<AttendancePeriod>> GetAttendancePeriodsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_attendance_period_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<AttendancePeriod>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
