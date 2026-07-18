using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class AttendanceCodeRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<AttendanceCode>(factory, authorizationService), IAttendanceCodeRepository
{
    public async Task<IList<AttendanceCode>> GetByIdsAsync(IEnumerable<Guid> attendanceCodeIds,
        CancellationToken cancellationToken)
    {
        var sql = "[dbo].[usp_attendance_code_get_by_ids]";

        var p = new { attendanceCodeIds = attendanceCodeIds.ToGuidTvp() };

        using var conn = _factory.Create();

        var rows = await conn.ExecuteStoredProcedureAsync<AttendanceCode>(sql, p,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }

    public async Task<IList<AttendanceCode>> GetActiveAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var rows = await conn.ExecuteStoredProcedureAsync<AttendanceCode>(
            "[dbo].[usp_attendance_code_get_active]", parameters: null,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }
}
