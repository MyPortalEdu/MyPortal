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
/// Manage houses within an academic year — the cross-year-group affiliation that
/// students belong to for events, points, and competitions. Houses run alongside
/// year groups in the pastoral structure.
/// </summary>
public sealed class HousesController : BaseApiController
{
    private readonly IHouseService _houseService;

    public HousesController(ProblemDetailsFactory problemFactory, ILogger<HousesController> logger,
        IHouseService houseService) : base(problemFactory, logger)
    {
        _houseService = houseService;
    }

    /// <summary>Page through house summaries for an academic year.</summary>
    /// <remarks>
    /// Supports server-side filtering, sorting, and paging. Page size is clamped
    /// server-side (default 25, max 100).
    /// </remarks>
    /// <param name="academicYearId">The academic year to scope the results to.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(PageResult<HouseSummaryResponse>), 200)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] Guid academicYearId,
        [FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _houseService.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the full details of a house by id.</summary>
    /// <param name="houseId">The id of the house.</param>
    [HttpGet("{houseId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(HouseDetailsResponse), 200)]
    public async Task<IActionResult> GetDetailsByIdAsync([FromRoute] Guid houseId)
    {
        var result = await _houseService.GetDetailsByIdAsync(houseId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a new house within an academic year.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] HouseUpsertRequest model)
    {
        var id = await _houseService.CreateAsync(model, CancellationToken);

        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a house's metadata.</summary>
    /// <param name="houseId">The id of the house to update.</param>
    /// <param name="model">The updated metadata.</param>
    [HttpPut("{houseId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid houseId, [FromBody] HouseUpsertRequest model)
    {
        await _houseService.UpdateAsync(houseId, model, CancellationToken);

        return NoContent();
    }

    /// <summary>Delete a house.</summary>
    /// <remarks>Fails if the house still has dependent supervisor assignments.</remarks>
    /// <param name="houseId">The id of the house to delete.</param>
    [HttpDelete("{houseId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid houseId)
    {
        await _houseService.DeleteAsync(houseId, CancellationToken);

        return NoContent();
    }
}
