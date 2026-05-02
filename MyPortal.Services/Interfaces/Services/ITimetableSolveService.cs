using MyPortal.Core.Entities;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Interfaces.Services;

public interface ITimetableSolveService
{
    /// Controller path. Permission-checks, writes a Run row with Status=Queued, enqueues the
    /// work item for the BackgroundService to pick up, and returns immediately. The returned
    /// run can be polled via the GET /runs/{runId} endpoint to track progress.
    Task<TimetableRun> QueueRunAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken);

    /// Worker path. Called from the BackgroundService once a queued item is dequeued. Flips
    /// the Run from Queued → Running, executes the solve, persists results, and marks the Run
    /// as Succeeded/Failed. No permission check — the queue acts as the trust boundary, having
    /// already enforced it at QueueRunAsync time.
    Task ExecuteRunAsync(Guid runId, Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken);
}
