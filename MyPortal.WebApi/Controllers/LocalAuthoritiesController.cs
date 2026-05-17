using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.School;
using MyPortal.Services.Interfaces.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Paged catalogue of local authorities. The list is large enough (~150 LAs)
/// that the school-details page uses a picker rather than a flat dropdown.
/// </summary>
public sealed class LocalAuthoritiesController : BaseApiController
{
    private readonly ILocalAuthorityService _service;

    public LocalAuthoritiesController(ProblemDetailsFactory problemFactory, ILogger<LocalAuthoritiesController> logger,
        ILocalAuthorityService service) : base(problemFactory, logger)
    {
        _service = service;
    }

    /// <summary>Page through local-authority summaries.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Agencies.ViewAgencies)]
    [ProducesResponseType(typeof(PageResult<LocalAuthoritySummaryResponse>), 200)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _service.GetSummariesAsync(options.FilterOptions, options.SortOptions, options.PageOptions,
            CancellationToken);

        return Ok(result);
    }
}
