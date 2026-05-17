using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Services.Interfaces.Curriculum;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage academic years — the time-bounded containers that own a school's terms,
/// attendance periods, school holidays, and timetable cycle. Most curriculum and
/// attendance data hangs off an academic year.
/// </summary>
public sealed class AcademicYearsController : BaseApiController
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(ProblemDetailsFactory problemFactory,
        ILogger<AcademicYearsController> logger, IAcademicYearService academicYearService)
        : base(problemFactory, logger)
    {
        _academicYearService = academicYearService;
    }

    /// <summary>List all academic years for the local school.</summary>
    /// <remarks>
    /// Returns lightweight summaries (id, name, start/end, IsLocked). Use the by-id
    /// endpoint for the full structure of any specific year. Requires
    /// <c>Curriculum.ViewAcademicYears</c>.
    /// </remarks>
    [HttpGet]
    // Edit implies read — editors must be able to list years to navigate to the
    // ones they want to update. Including both lets the FE gate the editor list
    // page on Edit alone without 403'ing the data call beneath it.
    [Permission(PermissionMode.RequireAny,
        Permissions.Curriculum.ViewAcademicYears,
        Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(IList<AcademicYearSummaryResponse>), 200)]
    public async Task<IActionResult> ListAsync()
    {
        var result = await _academicYearService.ListAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the academic year that's currently in progress, if any.</summary>
    /// <remarks>
    /// Returns <c>204 No Content</c> if no academic year covers today's date —
    /// for example, before the first AY has been seeded or during a summer gap.
    /// Useful for the SPA shell to default the AY picker.
    /// </remarks>
    [HttpGet("current")]
    [Permission(PermissionMode.RequireAny,
        Permissions.Curriculum.ViewAcademicYears,
        Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(AcademicYearSummaryResponse), 200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetCurrentAsync()
    {
        var result = await _academicYearService.GetCurrentAsync(CancellationToken);
        // Null = no AY has started yet. 204 keeps the contract simple — clients
        // either get an AY summary body or know there isn't one.
        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Get the full details of an academic year by id.</summary>
    /// <param name="academicYearId">The id of the academic year to look up.</param>
    [HttpGet("{academicYearId:guid}")]
    [Permission(PermissionMode.RequireAny,
        Permissions.Curriculum.ViewAcademicYears,
        Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(AcademicYearDetailsResponse), 200)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid academicYearId)
    {
        var result = await _academicYearService.GetByIdAsync(academicYearId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new academic year.</summary>
    /// <remarks>
    /// Creates the year as a container; terms, attendance periods, and school
    /// holidays are added separately afterwards. Requires
    /// <c>Curriculum.EditAcademicYears</c> and a Staff user type.
    /// </remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] AcademicYearUpsertRequest model)
    {
        var id = await _academicYearService.CreateAcademicYear(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update an existing academic year's metadata.</summary>
    /// <remarks>
    /// Updates name, start/end dates, timetable cycle length, and similar metadata.
    /// Does not touch downstream entities (terms, periods, holidays).
    /// </remarks>
    /// <param name="academicYearId">The id of the academic year to update.</param>
    /// <param name="model">The new metadata.</param>
    [HttpPut("{academicYearId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid academicYearId,
        [FromBody] AcademicYearUpsertRequest model)
    {
        var id = await _academicYearService.UpdateAcademicYear(academicYearId, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Delete an academic year.</summary>
    /// <remarks>
    /// Fails if the year has downstream data (attendance marks, timetable assignments,
    /// student group memberships, etc.). Inspect downstream usage first via the
    /// related controllers before attempting a delete.
    /// </remarks>
    /// <param name="academicYearId">The id of the academic year to delete.</param>
    [HttpDelete("{academicYearId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid academicYearId)
    {
        await _academicYearService.DeleteAcademicYear(academicYearId, CancellationToken);
        return NoContent();
    }
}
