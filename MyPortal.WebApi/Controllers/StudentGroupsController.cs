using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Services.Interfaces.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>Student-group listing endpoints.</summary>
public sealed class StudentGroupsController(
    ProblemDetailsFactory problemFactory,
    ILogger<StudentGroupsController> logger,
    IStudentGroupService service)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Page through student-group summaries for an academic year.</summary>
    /// <remarks>Includes a <c>Kind</c> column for subtype filtering.</remarks>
    /// <param name="academicYearId">The academic year to scope to.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    [ProducesResponseType(typeof(PageResult<StudentGroupSummaryResponse>), 200)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] Guid academicYearId,
        [FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await service.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }
}
