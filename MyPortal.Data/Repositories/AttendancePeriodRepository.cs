using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

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

        var result = await conn.ExecuteStoredProcedureAsync<AttendancePeriod>(
            "[dbo].[sp_attendance_period_get_by_academic_year_id]",
            new { academicYearId }, cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[sp_attendance_period_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    private (IDbConnection conn, bool owns) AcquireConnection(IDbTransaction? transaction)
    {
        if (transaction?.Connection is { } shared)
        {
            return (shared, false);
        }

        return (_factory.Create(), true);
    }
}
