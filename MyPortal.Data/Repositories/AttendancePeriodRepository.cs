using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class AttendancePeriodRepository : EntityRepository<AttendancePeriod>, IAttendancePeriodRepository
{
    public AttendancePeriodRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<AttendancePeriod>> GetAttendancePeriodsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_attendance_period_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<AttendancePeriod>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_attendance_period_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
