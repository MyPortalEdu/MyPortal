using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.School;
using MyPortal.Services.Interfaces.School;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Endpoints for school-level metadata. Single-tenant deployments have one local
/// school; the "local" endpoints return that.
/// </summary>
public class SchoolsController : BaseApiController
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(ProblemDetailsFactory problemFactory, ILogger<SchoolsController> logger,
        ISchoolService schoolService) : base(problemFactory, logger)
    {
        _schoolService = schoolService;
    }

    /// <summary>Get the local school's name as a plain string.</summary>
    /// <remarks>
    /// Used by the SPA shell for the title bar and the login page header. Returns
    /// an empty string (not 404) if no school has been configured yet, so the UI
    /// can render without a special case.
    /// </remarks>
    [HttpGet("local/name")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GetLocalSchoolName()
    {
        var school = await _schoolService.GetLocalSchoolDetailsAsync(CancellationToken);

        return Ok(school?.Name ?? "");
    }

    [HttpGet("local/details")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    public async Task<IActionResult> GetLocalSchool()
    {
        var school = await _schoolService.GetLocalSchoolDetailsAsync(CancellationToken);
        
        return school != null ? Ok(school.Name) : NoContent();
    }

    [HttpPost("local/details")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.EditAgencies)]
    public async Task<IActionResult> SaveLocalSchoolDetails([FromBody] SchoolUpsertRequest model)
    {
        var result = await _schoolService.CreateOrUpdateLocalSchoolAsync(model, CancellationToken);
        
        return Ok(result);
    }
}