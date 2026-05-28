using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Endpoints for browsing people across the various subtypes (staff, students,
/// contacts, agents). Currently surfaces only the staff slice — used by the
/// head-teacher picker on the school-details page.
/// </summary>
public sealed class PeopleController : BaseApiController
{
    private readonly IStaffMemberService _staffMemberService;

    public PeopleController(ProblemDetailsFactory problemFactory, ILogger<PeopleController> logger,
        IStaffMemberService staffMemberService) : base(problemFactory, logger)
    {
        _staffMemberService = staffMemberService;
    }

    /// <summary>Page through staff-member summaries for the staff picker.</summary>
    /// <remarks>
    /// Permission gating lives on the service (<c>Staff.ViewAllStaffBasicDetails</c>) —
    /// consumers such as the school-details head-teacher picker must hold it.
    /// </remarks>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet("staff")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(PageResult<StaffMemberSummaryResponse>), 200)]
    public async Task<IActionResult> GetStaffMembersAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _staffMemberService.GetStaffMembersAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }
}
