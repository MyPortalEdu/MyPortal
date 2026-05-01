using System.Data;
using System.Transactions;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Enums;
using MyPortal.Data.Interfaces.Repositories;
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
            Status = TimetableRunStatus.Running,
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
        using var tx = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.TimetableAssignments WHERE TimetableId = @timetableId;",
            new { timetableId }, cancellationToken: cancellationToken));

        if (assignments.Count > 0)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO dbo.TimetableAssignments
                    (Id, TimetableId, CurriculumBlockId, SlotIndex, ClassId, TeacherId,
                     RoomId, StartAttendancePeriodId, Size)
                  VALUES
                    (@Id, @TimetableId, @CurriculumBlockId, @SlotIndex, @ClassId, @TeacherId,
                     @RoomId, @StartAttendancePeriodId, @Size);",
                assignments, cancellationToken: cancellationToken));
        }

        tx.Complete();
    }

    public async Task<TimetableRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<TimetableRun>(new CommandDefinition(
            "SELECT * FROM dbo.TimetableRuns WHERE Id = @runId;",
            new { runId }, cancellationToken: cancellationToken));
    }
}
