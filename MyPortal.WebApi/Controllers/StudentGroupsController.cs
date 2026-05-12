using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Services.Interfaces.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Cross-subtype listing for student groups. Subtypes (Houses, YearGroups,
/// RegGroups, CurriculumGroups, …) each have their own controller for CRUD;
/// this controller exists so pickers (bulletin audiences, future schedulers,
/// etc.) can browse the lot through one paged endpoint with a Kind column.
/// </summary>
public sealed class StudentGroupsController : BaseApiController
{
    private readonly IStudentGroupService _service;

    public StudentGroupsController(ProblemDetailsFactory problemFactory,
        ILogger<StudentGroupsController> logger, IStudentGroupService service)
        : base(problemFactory, logger)
    {
        _service = service;
    }

    /// <summary>Page through unified student-group summaries for an academic year.</summary>
    /// <remarks>
    /// Server-side filter / sort / page via QueryKit. The <c>Kind</c> column
    /// (House / YearGroup / RegGroup / CurriculumGroup / Other) is a CASE
    /// expression in the underlying query — clients filter on it numerically
    /// using the values from <see cref="MyPortal.Common.Enums.StudentGroupKind"/>.
    /// </remarks>
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

        var result = await _service.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }
}
