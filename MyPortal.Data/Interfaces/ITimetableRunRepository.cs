using MyPortal.Common.Enums;
using MyPortal.Core.Entities;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface ITimetableRunRepository
{
    Task<TimetableRun> CreateRunAsync(Guid timetableId, Guid triggeredById, string? inputSnapshot,
        CancellationToken cancellationToken);

    Task MarkRunCompletedAsync(Guid runId, TimetableRunStatus status, string? diagnostic,
        CancellationToken cancellationToken);

    /// Replaces the timetable's draft assignments atomically — old rows deleted, new rows
    /// inserted in a single transaction. Used after a successful solve.
    Task ReplaceAssignmentsAsync(Guid timetableId, IReadOnlyList<TimetableAssignment> assignments,
        CancellationToken cancellationToken);

    Task<TimetableRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken);

    Task UpdateRunStatusAsync(Guid runId, TimetableRunStatus status, CancellationToken cancellationToken);

    /// Marks any Queued/Running rows as Failed with a "host restart" diagnostic. Called once
    /// when the worker spins up — without it, runs orphaned by a host crash would stay in a
    /// non-terminal state forever and the polling endpoint would never finish.
    Task<int> MarkOrphanedRunsFailedAsync(CancellationToken cancellationToken);
}
