using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Timetabler;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

public sealed class TimetablesController : BaseApiController<TimetablesController>
{
    private readonly ITimetableService _service;
    private readonly ITimetableSolveService _solveService;

    public TimetablesController(ProblemDetailsFactory problemFactory, ILogger<TimetablesController> logger,
        ITimetableService service, ITimetableSolveService solveService) : base(problemFactory, logger)
    {
        _service = service;
        _solveService = solveService;
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    public async Task<IActionResult> CreateAsync([FromBody] TimetableUpsertRequest model)
    {
        var id = await _service.CreateDraftAsync(model, CancellationToken);
        return Ok(new { id });
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    public async Task<IActionResult> ListAsync([FromQuery] Guid academicYearId)
    {
        var result = await _service.ListByAcademicYearAsync(academicYearId, CancellationToken);
        return Ok(result);
    }

    [HttpGet("{timetableId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.GetByIdAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    [HttpGet("{timetableId:guid}/assignments")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    public async Task<IActionResult> ListAssignmentsAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.ListAssignmentsAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    [HttpGet("{timetableId:guid}/runs")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    public async Task<IActionResult> ListRunsAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.ListRunsAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    [HttpGet("{timetableId:guid}/runs/{runId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    public async Task<IActionResult> GetRunAsync([FromRoute] Guid timetableId, [FromRoute] Guid runId)
    {
        // timetableId is in the route for resource-shape consistency; the run is identified by
        // its own id. Service implementations may want to assert membership later.
        var result = await _service.GetRunAsync(runId, CancellationToken);
        return Ok(result);
    }

    [HttpPost("{timetableId:guid}/runs")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    public async Task<IActionResult> RunAsync([FromRoute] Guid timetableId,
        [FromBody] TimetableRunRequest model)
    {
        // Synchronous v1 — callers should expect this to take minutes for full-school inputs.
        // A job-queue front-end can wrap this once we have one.
        var run = await _solveService.RunAsync(timetableId, model.WeekPatternId, CancellationToken);
        return Ok(new
        {
            id = run.Id,
            timetableId = run.TimetableId,
            status = (int)run.Status,
            startedAt = run.StartedAt,
            completedAt = run.CompletedAt,
            solverDiagnostic = run.SolverDiagnostic,
        });
    }

    [HttpPost("{timetableId:guid}/apply")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    public async Task<IActionResult> ApplyAsync([FromRoute] Guid timetableId,
        [FromBody] TimetableApplyRequest model)
    {
        await _service.ApplyAsync(timetableId, model, CancellationToken);
        return NoContent();
    }

    [HttpPost("{timetableId:guid}/discard")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    public async Task<IActionResult> DiscardAsync([FromRoute] Guid timetableId)
    {
        await _service.DiscardAsync(timetableId, CancellationToken);
        return NoContent();
    }
}
