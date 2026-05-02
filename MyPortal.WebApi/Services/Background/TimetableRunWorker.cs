using MyPortal.Data.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Services.Timetabler;

namespace MyPortal.WebApi.Services.Background;

/// Long-lived hosted service that drains TimetableRunQueue and dispatches each item to the
/// solve service. One scope per work item so DbContext / scoped services behave normally.
public class TimetableRunWorker : BackgroundService
{
    private readonly TimetableRunQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TimetableRunWorker> _logger;

    public TimetableRunWorker(TimetableRunQueue queue, IServiceProvider serviceProvider,
        ILogger<TimetableRunWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Startup sweep. The previous host might have died while runs were Queued/Running —
        // those rows are dead but still non-terminal. Mark them Failed so the polling
        // endpoint doesn't spin and admins can see what happened.
        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var runRepo = scope.ServiceProvider.GetRequiredService<ITimetableRunRepository>();
            var swept = await runRepo.MarkOrphanedRunsFailedAsync(stoppingToken);
            if (swept > 0)
            {
                _logger.LogWarning("Marked {count} orphaned timetable runs as Failed at worker startup.",
                    swept);
            }
        }

        // Single-reader by design — CP-SAT pegs all CPU cores already, so parallel solves
        // wouldn't speed anything up. Items are processed in FIFO order.
        await foreach (var item in _queue.ReadAllAsync(stoppingToken))
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var solveService = scope.ServiceProvider.GetRequiredService<ITimetableSolveService>();
            try
            {
                await solveService.ExecuteRunAsync(item.RunId, item.TimetableId, item.WeekPatternId,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                // ExecuteRunAsync already records Failed status with the diagnostic on the Run
                // row, so we just need to keep the worker alive. Without this catch, an
                // unhandled exception would tear down the BackgroundService and stop the queue
                // dead.
                _logger.LogError(ex, "Background run worker swallowed exception for run {runId}.",
                    item.RunId);
            }
        }
    }
}
