using System.Data;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class AttendanceMarkRepository(IDbConnectionFactory factory) : IAttendanceMarkRepository
{
    public async Task<BulkAttendanceMarksResponse?> GetBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        using var conn = factory.Create();

        var command = new CommandDefinition("[dbo].[usp_attendance_marks_get_bulk]",
            new { studentGroupId, from, to },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<BulkAttendanceMarksResponse>();
        if (header is null)
        {
            return null;
        }

        // Order matches the SP: header, periods, students, marks.
        header.Periods  = (await reader.ReadAsync<BulkAttendancePeriodInstanceResponse>()).ToList();
        header.Students = (await reader.ReadAsync<BulkAttendanceStudentResponse>()).ToList();
        header.Marks    = (await reader.ReadAsync<RegisterMarkResponse>()).ToList();

        return header;
    }

    public async Task SubmitBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        IReadOnlyCollection<BulkAttendanceMarkUpsert> marks, CancellationToken cancellationToken)
    {
        using var conn = factory.Create();

        var command = new CommandDefinition("[dbo].[usp_attendance_marks_submit_bulk]",
            new
            {
                studentGroupId,
                from,
                to,
                marks = MarksAsTvp(marks)
            },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        await conn.ExecuteAsync(command);
    }

    private static SqlMapper.ICustomQueryParameter MarksAsTvp(IReadOnlyCollection<BulkAttendanceMarkUpsert> marks)
    {
        var table = new DataTable();
        table.Columns.Add("StudentId",          typeof(Guid));
        table.Columns.Add("AttendanceWeekId",   typeof(Guid));
        table.Columns.Add("AttendancePeriodId", typeof(Guid));
        table.Columns.Add("AttendanceCodeId",   typeof(Guid));
        table.Columns.Add("Comments",           typeof(string));
        table.Columns.Add("MinutesLate",        typeof(int));

        foreach (var m in marks)
        {
            table.Rows.Add(
                m.StudentId,
                m.AttendanceWeekId,
                m.AttendancePeriodId,
                (object?)m.AttendanceCodeId ?? DBNull.Value,
                (object?)m.Comments         ?? DBNull.Value,
                (object?)m.MinutesLate      ?? DBNull.Value);
        }

        return table.AsTableValuedParameter("dbo.BulkAttendanceMarkList");
    }
}
