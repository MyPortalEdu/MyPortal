using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.WebApi.Controllers;

/// <summary>Read-only catalogue of intake types — used by the school-details dropdown.</summary>
public sealed class IntakeTypesController(
    ProblemDetailsFactory problemFactory,
    ILogger<IntakeTypesController> logger,
    ILookupService lookupService)
    : BaseApiController(problemFactory, logger)
{
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(IList<LookupResponse>), 200)]
    public async Task<IActionResult> GetIntakeTypes()
    {
        var result = await lookupService.GetIntakeTypesAsync(CancellationToken);
        return Ok(result);
    }
}
