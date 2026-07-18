using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.WebApi.Controllers;

/// <summary>Read-only catalogue of pay zones — used by the school-details dropdown.</summary>
public sealed class PayZonesController(
    ProblemDetailsFactory problemFactory,
    ILogger<PayZonesController> logger,
    ILookupService lookupService)
    : BaseApiController(problemFactory, logger)
{
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(IList<LookupResponse>), 200)]
    public async Task<IActionResult> GetPayZones()
    {
        var result = await lookupService.GetPayZonesAsync(CancellationToken);
        return Ok(result);
    }
}
