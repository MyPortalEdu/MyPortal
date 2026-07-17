using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>People lookup endpoints.</summary>
public sealed class PeopleController : BaseApiController
{
    private readonly IStaffMemberService _staffMemberService;
    private readonly IPersonService _personService;

    public PeopleController(ProblemDetailsFactory problemFactory, ILogger<PeopleController> logger,
        IStaffMemberService staffMemberService, IPersonService personService) : base(problemFactory, logger)
    {
        _staffMemberService = staffMemberService;
        _personService = personService;
    }

    /// <summary>Page through staff-member summaries for the picker.</summary>
    /// <remarks>Requires <c>Staff.ViewAllStaffBasicDetails</c>.</remarks>
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

    /// <summary>Search People by name to link to a user account.</summary>
    /// <param name="query">The search term (min 2 characters; shorter returns empty).</param>
    [HttpGet("search")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(typeof(IReadOnlyList<PersonSearchResponse>), 200)]
    public async Task<IActionResult> SearchPeopleAsync([FromQuery] string query)
    {
        var result = await _personService.SearchAsync(query, CancellationToken);

        return Ok(result);
    }
}
