using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.School;
using MyPortal.Services.Interfaces.School;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>School metadata endpoints.</summary>
public class SchoolsController(
    ProblemDetailsFactory problemFactory,
    ILogger<SchoolsController> logger,
    ISchoolService schoolService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get the local school's name.</summary>
    /// <remarks>Returns an empty string if no school has been configured yet.</remarks>
    [HttpGet("local/name")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GetLocalSchoolName()
    {
        var school = await schoolService.GetLocalSchoolDetailsAsync(CancellationToken);

        return Ok(school?.Name ?? "");
    }

    /// <summary>
    /// Get the local school's details.'
    /// </summary>
    [HttpGet("local/details")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(SchoolDetailsResponse), 200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetLocalSchool()
    {
        var school = await schoolService.GetLocalSchoolDetailsAsync(CancellationToken);

        return school != null ? Ok(school) : NoContent();
    }

    /// <summary>
    /// Save the local school's details.
    /// </summary>
    /// <param name="model">The request model containing the local school's new details.</param>
    [HttpPost("local/details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.EditAgencies)]
    public async Task<IActionResult> SaveLocalSchoolDetails([FromBody] SchoolUpsertRequest model)
    {
        var result = await schoolService.CreateOrUpdateLocalSchoolAsync(model, CancellationToken);
        
        return Ok(result);
    }
}
