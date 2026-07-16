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

/// <summary>Year-group endpoints.</summary>
public sealed class YearGroupsController : BaseApiController
{
    private readonly IYearGroupService _yearGroupService;

    public YearGroupsController(ProblemDetailsFactory problemFactory, ILogger<YearGroupsController> logger,
        IYearGroupService yearGroupService) : base(problemFactory, logger)
    {
        _yearGroupService = yearGroupService;
    }

    /// <summary>Page through year-group summaries for an academic year.</summary>
    /// <remarks>Supports server-side filtering, sorting, and paging. Page size is clamped.</remarks>
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

    /// <summary>Get a year group by id.</summary>
    /// <param name="yearGroupId">The id of the year group.</param>
    [HttpGet("{yearGroupId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(YearGroupDetailsResponse), 200)]
    public async Task<IActionResult> GetDetailsByIdAsync([FromRoute] Guid yearGroupId)
    {
        var result = await _yearGroupService.GetDetailsByIdAsync(yearGroupId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a year group.</summary>
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
    /// <remarks>Fails if the year group still has dependent records.</remarks>
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
