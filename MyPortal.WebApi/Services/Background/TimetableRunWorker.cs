using MyPortal.Data.Interfaces;
using MyPortal.Services.Curriculum.Timetable;
using MyPortal.Services.Interfaces.Timetable;

namespace MyPortal.WebApi.Services.Background;

/// Long-lived hosted service that drains TimetableRunQueue and dispatches each item to the
/// solve service. One scope per work item so DbContext / scoped services behave normally.
public class TimetableRunWorker(
    TimetableRunQueue queue,
    IServiceProvider serviceProvider,
    ILogger<TimetableRunWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var runRepo = scope.ServiceProvider.GetRequiredService<ITimetableRunRepository>();
            var swept = await runRepo.MarkOrphanedRunsFailedAsync(stoppingToken);
            if (swept > 0)
            {
                logger.LogWarning("Marked {count} orphaned timetable runs as Failed at worker startup.",
                    swept);
            }
        }

        await foreach (var item in queue.ReadAllAsync(stoppingToken))
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var solveService = scope.ServiceProvider.GetRequiredService<ITimetableSolveService>();
            try
            {
                await solveService.ExecuteRunAsync(item.RunId, item.TimetableId, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background run worker swallowed exception for run {runId}.",
                    item.RunId);
            }
        }
    }
}
