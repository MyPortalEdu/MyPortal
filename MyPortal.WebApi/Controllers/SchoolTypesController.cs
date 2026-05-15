using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.WebApi.Controllers;

/// <summary>Read-only catalogue of school types — used by the school-details dropdown.</summary>
public sealed class SchoolTypesController : BaseApiController
{
    private readonly ILookupService _lookupService;

    public SchoolTypesController(ProblemDetailsFactory problemFactory, ILogger<SchoolTypesController> logger,
        ILookupService lookupService) : base(problemFactory, logger)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(IList<LookupResponse>), 200)]
    public async Task<IActionResult> GetSchoolTypes()
    {
        var result = await _lookupService.GetSchoolTypesAsync(CancellationToken);
        return Ok(result);
    }
}
