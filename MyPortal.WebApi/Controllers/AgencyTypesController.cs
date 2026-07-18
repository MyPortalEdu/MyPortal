using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.WebApi.Controllers;

/// <summary>Read-only catalogue of agency types — used by the school-details dropdown.</summary>
public sealed class AgencyTypesController(
    ProblemDetailsFactory problemFactory,
    ILogger<AgencyTypesController> logger,
    ILookupService lookupService)
    : BaseApiController(problemFactory, logger)
{
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(IList<LookupResponse>), 200)]
    public async Task<IActionResult> GetAgencyTypes()
    {
        var result = await lookupService.GetAgencyTypesAsync(CancellationToken);
        return Ok(result);
    }
}
