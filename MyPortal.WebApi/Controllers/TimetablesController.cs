using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Timetabler;
using MyPortal.Services.Interfaces.Timetable;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage timetables — drafts, solver runs, pins, and applying a draft to live
/// session data. A timetable lives inside an academic year; runs are async solver
/// jobs queued onto a background worker.
/// </summary>
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

    /// <summary>Create a new draft timetable for an academic year.</summary>
    /// <remarks>
    /// The new timetable is empty until you queue a run, or hand-edit assignments.
    /// </remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] TimetableUpsertRequest model)
    {
        var id = await _service.CreateDraftAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>List all timetables (draft and applied) within an academic year.</summary>
    /// <param name="academicYearId">The academic year to scope to.</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(IList<TimetableSummaryResponse>), 200)]
    public async Task<IActionResult> ListAsync([FromQuery] Guid academicYearId)
    {
        var result = await _service.ListByAcademicYearAsync(academicYearId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get a timetable's metadata.</summary>
    /// <param name="timetableId">The id of the timetable.</param>
    [HttpGet("{timetableId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(TimetableDetailsResponse), 200)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.GetByIdAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    /// <summary>List the assignments (period × class × room × teacher) for a timetable.</summary>
    /// <remarks>
    /// For draft timetables this returns the latest solver run's assignments. For
    /// applied timetables it returns what's currently driving lessons.
    /// </remarks>
    /// <param name="timetableId">The id of the timetable.</param>
    [HttpGet("{timetableId:guid}/assignments")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(IList<TimetableAssignmentResponse>), 200)]
    public async Task<IActionResult> ListAssignmentsAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.ListAssignmentsAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    /// <summary>List solver runs for a timetable.</summary>
    /// <remarks>Each run records the queued/started/completed timestamps and solver diagnostics.</remarks>
    /// <param name="timetableId">The id of the timetable.</param>
    [HttpGet("{timetableId:guid}/runs")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(IList<TimetableRunResponse>), 200)]
    public async Task<IActionResult> ListRunsAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.ListRunsAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the status and diagnostics of a single solver run.</summary>
    /// <remarks>Poll this after queuing a run to track progress and pick up the diagnostic on completion.</remarks>
    /// <param name="timetableId">The owning timetable (kept in the route for resource-shape consistency).</param>
    /// <param name="runId">The id of the run.</param>
    [HttpGet("{timetableId:guid}/runs/{runId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(TimetableRunResponse), 200)]
    public async Task<IActionResult> GetRunAsync([FromRoute] Guid timetableId, [FromRoute] Guid runId)
    {
        // timetableId is in the route for resource-shape consistency; the run is identified by
        // its own id. Service implementations may want to assert membership later.
        var result = await _service.GetRunAsync(runId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Queue a solver run for the timetable.</summary>
    /// <remarks>
    /// Returns <c>202 Accepted</c> immediately with the new run's id. The
    /// background worker picks the run up and executes the solve; clients poll
    /// <c>GET /runs/{runId}</c> for status.
    /// </remarks>
    /// <param name="timetableId">The id of the timetable to solve.</param>
    [HttpPost("{timetableId:guid}/runs")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(typeof(TimetableRunResponse), 202)]
    public async Task<IActionResult> RunAsync([FromRoute] Guid timetableId)
    {
        // Returns immediately — the BackgroundService picks up the queued item and
        // executes the solve. Caller polls GET /runs/{runId} for status.
        var run = await _solveService.QueueRunAsync(timetableId, CancellationToken);
        return Accepted(new
        {
            id = run.Id,
            timetableId = run.TimetableId,
            status = (int)run.Status,
            startedAt = run.StartedAt,
            completedAt = run.CompletedAt,
            solverDiagnostic = run.SolverDiagnostic,
        });
    }

    /// <summary>Apply a draft timetable's solved assignments to live session data.</summary>
    /// <remarks>
    /// Replaces the existing live session data for the academic year. Cannot be
    /// undone; the previous live data is overwritten. Use carefully.
    /// </remarks>
    /// <param name="timetableId">The id of the draft timetable to apply.</param>
    /// <param name="model">Apply options (e.g. effective-from date, dry-run flag).</param>
    [HttpPost("{timetableId:guid}/apply")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ApplyAsync([FromRoute] Guid timetableId,
        [FromBody] TimetableApplyRequest model)
    {
        await _service.ApplyAsync(timetableId, model, CancellationToken);
        return NoContent();
    }

    /// <summary>Discard an unapplied draft timetable.</summary>
    /// <remarks>Removes the timetable and any associated runs/pins. Applied timetables cannot be discarded — you'd need to apply a replacement instead.</remarks>
    /// <param name="timetableId">The id of the draft to discard.</param>
    [HttpPost("{timetableId:guid}/discard")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DiscardAsync([FromRoute] Guid timetableId)
    {
        await _service.DiscardAsync(timetableId, CancellationToken);
        return NoContent();
    }

    // ─── pins ─────────────────────────────────────────────────────────────

    /// <summary>List the pins (forced placements) on a timetable.</summary>
    /// <remarks>Pins constrain the solver — pinned assignments stay fixed across runs.</remarks>
    /// <param name="timetableId">The id of the timetable.</param>
    [HttpGet("{timetableId:guid}/pins")]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.ViewTimetables)]
    [ProducesResponseType(typeof(IList<TimetablePinResponse>), 200)]
    public async Task<IActionResult> ListPinsAsync([FromRoute] Guid timetableId)
    {
        var result = await _service.ListPinsAsync(timetableId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Add a pin to a timetable.</summary>
    /// <remarks>
    /// Pins force a specific (class, period, room, teacher) combination. The next
    /// solver run will respect the pin — over-constraining can make a timetable
    /// unsolvable, in which case the run completes with diagnostic errors.
    /// </remarks>
    /// <param name="timetableId">The id of the timetable.</param>
    /// <param name="model">The pin to add.</param>
    [HttpPost("{timetableId:guid}/pins")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> AddPinAsync([FromRoute] Guid timetableId,
        [FromBody] TimetablePinUpsertRequest model)
    {
        var id = await _service.AddPinAsync(timetableId, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Remove a pin from a timetable.</summary>
    /// <param name="timetableId">The id of the timetable.</param>
    /// <param name="pinId">The id of the pin to remove.</param>
    [HttpDelete("{timetableId:guid}/pins/{pinId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Timetable.EditTimetables)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemovePinAsync([FromRoute] Guid timetableId,
        [FromRoute] Guid pinId)
    {
        await _service.RemovePinAsync(timetableId, pinId, CancellationToken);
        return NoContent();
    }
}
