using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Services.Interfaces.Pastoral;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage year groups within an academic year — the cohort layer of the pastoral
/// structure (e.g. "Year 7", "Year 11"). Reg groups belong to year groups; year
/// groups belong to academic years.
/// </summary>
public sealed class YearGroupsController : BaseApiController<YearGroupsController>
{
    private readonly IYearGroupService _yearGroupService;

    public YearGroupsController(ProblemDetailsFactory problemFactory, ILogger<YearGroupsController> logger,
        IYearGroupService yearGroupService) : base(problemFactory, logger)
    {
        _yearGroupService = yearGroupService;
    }

    /// <summary>Page through year-group summaries for an academic year.</summary>
    /// <remarks>
    /// Supports server-side filtering, sorting, and paging via the standard
    /// <c>filter</c>/<c>sort</c>/<c>page</c>/<c>pageSize</c> query parameters.
    /// Page size is clamped server-side (default 25, max 100).
    /// </remarks>
    /// <param name="academicYearId">The academic year to scope the results to.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(PageResult<YearGroupSummaryResponse>), 200)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] Guid academicYearId,
        [FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _yearGroupService.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the full details of a year group by id.</summary>
    /// <param name="yearGroupId">The id of the year group.</param>
    [HttpGet("{yearGroupId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(YearGroupDetailsResponse), 200)]
    public async Task<IActionResult> GetDetailsByIdAsync([FromRoute] Guid yearGroupId)
    {
        var result = await _yearGroupService.GetDetailsByIdAsync(yearGroupId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a new year group within an academic year.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] YearGroupUpsertRequest model)
    {
        var id = await _yearGroupService.CreateAsync(model, CancellationToken);

        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a year group's metadata.</summary>
    /// <param name="yearGroupId">The id of the year group to update.</param>
    /// <param name="model">The updated metadata.</param>
    [HttpPut("{yearGroupId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid yearGroupId,
        [FromBody] YearGroupUpsertRequest model)
    {
        await _yearGroupService.UpdateAsync(yearGroupId, model, CancellationToken);

        return NoContent();
    }

    /// <summary>Delete a year group.</summary>
    /// <remarks>
    /// Fails if the year group has dependent reg groups or supervisor assignments.
    /// </remarks>
    /// <param name="yearGroupId">The id of the year group to delete.</param>
    [HttpDelete("{yearGroupId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid yearGroupId)
    {
        await _yearGroupService.DeleteAsync(yearGroupId, CancellationToken);

        return NoContent();
    }
}
