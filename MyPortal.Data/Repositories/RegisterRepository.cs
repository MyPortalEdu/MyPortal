using System.Data;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces.Repositories;

namespace MyPortal.Data.Repositories;

public class RegisterRepository : IRegisterRepository
{
    private readonly IDbConnectionFactory _factory;

    public RegisterRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public Task<RegisterResponse?> GetLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        CancellationToken cancellationToken)
        => GetRegisterAsync("[dbo].[sp_register_get_lesson]",
            new { sessionPeriodId, attendanceWeekId }, cancellationToken);

    public Task<RegisterResponse?> GetRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId,
        Guid attendanceWeekId, CancellationToken cancellationToken)
        => GetRegisterAsync("[dbo].[sp_register_get_reg_group]",
            new { regGroupId, attendancePeriodId, attendanceWeekId }, cancellationToken);

    public Task SubmitLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        IReadOnlyCollection<SubmitMarkRequest> marks, CancellationToken cancellationToken)
        => SubmitAsync(new
        {
            sessionPeriodId,
            regGroupId = (Guid?)null,
            attendancePeriodId = (Guid?)null,
            attendanceWeekId,
            marks = MarksAsTvp(marks)
        }, cancellationToken);

    public Task SubmitRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId, Guid attendanceWeekId,
        IReadOnlyCollection<SubmitMarkRequest> marks, CancellationToken cancellationToken)
        => SubmitAsync(new
        {
            sessionPeriodId = (Guid?)null,
            regGroupId,
            attendancePeriodId,
            attendanceWeekId,
            marks = MarksAsTvp(marks)
        }, cancellationToken);

    private async Task<RegisterResponse?> GetRegisterAsync(string sql, object parameters,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition(sql, parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<RegisterResponse>();
        if (header is null)
        {
            return null;
        }

        header.Students = (await reader.ReadAsync<RegisterStudentResponse>()).ToList();
        header.Marks    = (await reader.ReadAsync<RegisterMarkResponse>()).ToList();

        return header;
    }

    private async Task SubmitAsync(object parameters, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition("[dbo].[sp_register_submit]", parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        await conn.ExecuteAsync(command);
    }

    private static SqlMapper.ICustomQueryParameter MarksAsTvp(IReadOnlyCollection<SubmitMarkRequest> marks)
    {
        var table = new DataTable();
        table.Columns.Add("StudentId",        typeof(Guid));
        table.Columns.Add("AttendanceCodeId", typeof(Guid));
        table.Columns.Add("Comments",         typeof(string));
        table.Columns.Add("MinutesLate",      typeof(int));

        foreach (var m in marks)
        {
            table.Rows.Add(m.StudentId, m.AttendanceCodeId,
                (object?)m.Comments    ?? DBNull.Value,
                (object?)m.MinutesLate ?? DBNull.Value);
        }

        return table.AsTableValuedParameter("dbo.AttendanceMarkList");
    }
}
