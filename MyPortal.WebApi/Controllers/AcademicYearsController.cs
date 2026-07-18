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

/// <summary>Academic-year endpoints.</summary>
public sealed class AcademicYearsController(
    ProblemDetailsFactory problemFactory,
    ILogger<AcademicYearsController> logger,
    IAcademicYearService academicYearService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>List academic years for the local school.</summary>
    /// <remarks>Returns lightweight summaries. Use the by-id endpoint for full details.</remarks>
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
        var result = await academicYearService.ListAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the current academic year, if any.</summary>
    /// <remarks>Returns <c>204 No Content</c> when no academic year covers today's date.</remarks>
    [HttpGet("current")]
    [Permission(PermissionMode.RequireAny,
        Permissions.Curriculum.ViewAcademicYears,
        Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(AcademicYearSummaryResponse), 200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetCurrentAsync()
    {
        var result = await academicYearService.GetCurrentAsync(CancellationToken);
        // Null = no AY has started yet. 204 keeps the contract simple — clients
        // either get an AY summary body or know there isn't one.
        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Get an academic year by id.</summary>
    /// <param name="academicYearId">The id of the academic year to look up.</param>
    [HttpGet("{academicYearId:guid}")]
    [Permission(PermissionMode.RequireAny,
        Permissions.Curriculum.ViewAcademicYears,
        Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(AcademicYearDetailsResponse), 200)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid academicYearId)
    {
        var result = await academicYearService.GetByIdAsync(academicYearId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Create an academic year.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] AcademicYearUpsertRequest model)
    {
        var id = await academicYearService.CreateAcademicYear(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update an academic year's metadata.</summary>
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
        var id = await academicYearService.UpdateAcademicYear(academicYearId, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Delete an academic year.</summary>
    /// <param name="academicYearId">The id of the academic year to delete.</param>
    [HttpDelete("{academicYearId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid academicYearId)
    {
        await academicYearService.DeleteAcademicYear(academicYearId, CancellationToken);
        return NoContent();
    }
}
