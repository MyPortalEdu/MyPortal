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

namespace MyPortal.Services.Timetabler;

public class TimetableSolveService : BaseService, ITimetableSolveService
{
    private readonly ITimetableSourceRepository _sourceRepository;
    private readonly ITimetableRunRepository _runRepository;
    private readonly TimetableInputBuilder _builder;
    private readonly ITimetableSolver _solver;

    public TimetableSolveService(IAuthorizationService authorizationService,
        ILogger<TimetableSolveService> logger,
        ITimetableSourceRepository sourceRepository,
        ITimetableRunRepository runRepository,
        TimetableInputBuilder builder,
        ITimetableSolver solver)
        : base(authorizationService, logger)
    {
        _sourceRepository = sourceRepository;
        _runRepository = runRepository;
        _builder = builder;
        _solver = solver;
    }

    public async Task<TimetableRun> RunAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var triggeredById = AuthorizationService.GetCurrentUserId()
            ?? throw new ForbiddenException("Not authenticated.");

        var run = await _runRepository.CreateRunAsync(timetableId, triggeredById,
            inputSnapshot: null, cancellationToken);

        Logger.LogInformation("Timetable run started: {runId} (timetable {timetableId})",
            run.Id, timetableId);

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
                await _runRepository.MarkRunCompletedAsync(run.Id, TimetableRunStatus.Succeeded,
                    output.Diagnostic, cancellationToken);

                Logger.LogInformation(
                    "Timetable run succeeded: {runId} ({assignmentCount} assignments, {status})",
                    run.Id, entities.Length, output.Status);
            }
            else
            {
                await _runRepository.MarkRunCompletedAsync(run.Id, TimetableRunStatus.Failed,
                    output.Diagnostic, cancellationToken);

                Logger.LogWarning("Timetable run failed: {runId} ({status} — {diagnostic})",
                    run.Id, output.Status, output.Diagnostic);
            }
        }
        catch (Exception ex)
        {
            // Surface the failure on the Run row so admins can read it via the polling endpoint
            // even though we're rethrowing.
            await _runRepository.MarkRunCompletedAsync(run.Id, TimetableRunStatus.Failed,
                ex.Message, CancellationToken.None);

            Logger.LogError(ex, "Timetable run threw: {runId}", run.Id);
            throw;
        }

        return await _runRepository.GetRunAsync(run.Id, cancellationToken)
               ?? throw new NotFoundException($"Run '{run.Id}' missing after completion.");
    }
}
