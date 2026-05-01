using MyPortal.Core.Entities;
using MyPortal.Core.Enums;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces.Repositories;

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
}
