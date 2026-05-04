using System.Data;
using Dapper;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class TimetableRunRepository : ITimetableRunRepository
{
    private readonly IDbConnectionFactory _factory;

    public TimetableRunRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TimetableRun> CreateRunAsync(Guid timetableId, Guid triggeredById,
        string? inputSnapshot, CancellationToken cancellationToken)
    {
        var run = new TimetableRun
        {
            Id = Guid.NewGuid(),
            TimetableId = timetableId,
            Status = TimetableRunStatus.Queued,
            StartedAt = DateTime.UtcNow,
            CompletedAt = null,
            SolverDiagnostic = null,
            InputSnapshot = inputSnapshot,
            TriggeredById = triggeredById,
        };

        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO dbo.TimetableRuns
                (Id, TimetableId, Status, StartedAt, CompletedAt, SolverDiagnostic, InputSnapshot, TriggeredById)
              VALUES
                (@Id, @TimetableId, @Status, @StartedAt, @CompletedAt, @SolverDiagnostic, @InputSnapshot, @TriggeredById);",
            run, cancellationToken: cancellationToken));

        return run;
    }

    public async Task MarkRunCompletedAsync(Guid runId, TimetableRunStatus status, string? diagnostic,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE dbo.TimetableRuns
                  SET Status = @status,
                      CompletedAt = @completedAt,
                      SolverDiagnostic = @diagnostic
                WHERE Id = @runId;",
            new { runId, status = (int)status, completedAt = DateTime.UtcNow, diagnostic },
            cancellationToken: cancellationToken));
    }

    public async Task ReplaceAssignmentsAsync(Guid timetableId,
        IReadOnlyList<TimetableAssignment> assignments, CancellationToken cancellationToken)
    {
        // Atomic swap: clearing previous-run output before inserting new rows means a partially
        // failed write can't leave a mix of stale + fresh assignments under the same Timetable.
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.TimetableAssignments WHERE TimetableId = @timetableId;",
            new { timetableId }, transaction: tx, cancellationToken: cancellationToken));

        if (assignments.Count > 0)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO dbo.TimetableAssignments
                    (Id, TimetableId, CurriculumBlockId, SlotIndex, ClassId, TeacherId,
                     RoomId, StartAttendancePeriodId, Size)
                  VALUES
                    (@Id, @TimetableId, @CurriculumBlockId, @SlotIndex, @ClassId, @TeacherId,
                     @RoomId, @StartAttendancePeriodId, @Size);",
                assignments, transaction: tx, cancellationToken: cancellationToken));
        }

        tx.Commit();
    }

    public async Task<TimetableRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<TimetableRun>(new CommandDefinition(
            "SELECT * FROM dbo.TimetableRuns WHERE Id = @runId;",
            new { runId }, cancellationToken: cancellationToken));
    }

    public async Task UpdateRunStatusAsync(Guid runId, TimetableRunStatus status,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE dbo.TimetableRuns SET Status = @status WHERE Id = @runId;",
            new { runId, status = (int)status }, cancellationToken: cancellationToken));
    }

    public async Task<int> MarkOrphanedRunsFailedAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE dbo.TimetableRuns
                  SET Status = @failed,
                      CompletedAt = SYSUTCDATETIME(),
                      SolverDiagnostic = 'Orphaned by host restart.'
                WHERE Status IN (@queued, @running);",
            new
            {
                queued  = (int)TimetableRunStatus.Queued,
                running = (int)TimetableRunStatus.Running,
                failed  = (int)TimetableRunStatus.Failed,
            }, cancellationToken: cancellationToken));
    }
}
