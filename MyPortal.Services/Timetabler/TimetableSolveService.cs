using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Core.Entities;
using MyPortal.Core.Enums;
using MyPortal.Data.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Timetabler.Models;
using MyPortal.Timetabler.Solver;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Timetabler;

public class TimetableSolveService : BaseService, ITimetableSolveService
{
    private readonly ITimetableSourceRepository _sourceRepository;
    private readonly ITimetableRunRepository _runRepository;
    private readonly TimetableInputBuilder _builder;
    private readonly ITimetableSolver _solver;
    private readonly TimetableRunQueue _queue;

    public TimetableSolveService(IAuthorizationService authorizationService,
        ILogger<TimetableSolveService> logger,
        ITimetableSourceRepository sourceRepository,
        ITimetableRunRepository runRepository,
        TimetableInputBuilder builder,
        ITimetableSolver solver,
        TimetableRunQueue queue)
        : base(authorizationService, logger)
    {
        _sourceRepository = sourceRepository;
        _runRepository = runRepository;
        _builder = builder;
        _solver = solver;
        _queue = queue;
    }

    public async Task<TimetableRun> QueueRunAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var triggeredById = AuthorizationService.GetCurrentUserId()
            ?? throw new ForbiddenException("Not authenticated.");

        var run = await _runRepository.CreateRunAsync(timetableId, triggeredById,
            inputSnapshot: null, cancellationToken);

        await _queue.EnqueueAsync(new TimetableRunWorkItem(run.Id, timetableId, weekPatternId),
            cancellationToken);

        Logger.LogInformation("Timetable run queued: {runId} (timetable {timetableId})",
            run.Id, timetableId);

        return run;
    }

    public async Task ExecuteRunAsync(Guid runId, Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken)
    {
        await _runRepository.UpdateRunStatusAsync(runId, TimetableRunStatus.Running, cancellationToken);

        Logger.LogInformation("Timetable run executing: {runId}", runId);

        try
        {
            var sources = await _sourceRepository.LoadAsync(timetableId, weekPatternId, cancellationToken);
            var input = _builder.Build(sources);
            var output = _solver.Solve(input);

            if (output.Status is SolveStatus.Feasible or SolveStatus.Optimal)
            {
                var entities = output.Assignments
                    .Select(a => new TimetableAssignment
                    {
                        Id = SqlConvention.SequentialGuid(),
                        TimetableId = timetableId,
                        CurriculumBlockId = Guid.Parse(a.BlockId),
                        SlotIndex = a.SlotIndex,
                        ClassId = Guid.Parse(a.ClassId),
                        TeacherId = Guid.Parse(a.TeacherId),
                        RoomId = string.IsNullOrEmpty(a.RoomId) ? null : Guid.Parse(a.RoomId),
                        StartAttendancePeriodId = Guid.Parse(a.PeriodIds[0]),
                        Size = a.PeriodIds.Count,
                    })
                    .ToArray();

                await _runRepository.ReplaceAssignmentsAsync(timetableId, entities, cancellationToken);
                await _runRepository.MarkRunCompletedAsync(runId, TimetableRunStatus.Succeeded,
                    output.Diagnostic, cancellationToken);

                Logger.LogInformation(
                    "Timetable run succeeded: {runId} ({assignmentCount} assignments, {status})",
                    runId, entities.Length, output.Status);
            }
            else
            {
                await _runRepository.MarkRunCompletedAsync(runId, TimetableRunStatus.Failed,
                    output.Diagnostic, cancellationToken);

                Logger.LogWarning("Timetable run failed: {runId} ({status} — {diagnostic})",
                    runId, output.Status, output.Diagnostic);
            }
        }
        catch (Exception ex)
        {
            // Record failure on the Run row using a fresh CancellationToken — even if the
            // outer cancellation fired (host shutdown), the polling endpoint should still see
            // a terminal status rather than a stuck Running.
            await _runRepository.MarkRunCompletedAsync(runId, TimetableRunStatus.Failed,
                ex.Message, CancellationToken.None);

            Logger.LogError(ex, "Timetable run threw: {runId}", runId);
            throw;
        }
    }
}
