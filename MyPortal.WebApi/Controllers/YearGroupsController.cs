using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Services.Interfaces.Pastoral;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

public sealed class YearGroupsController : BaseApiController<YearGroupsController>
{
    private readonly IYearGroupService _yearGroupService;

    public YearGroupsController(ProblemDetailsFactory problemFactory, ILogger<YearGroupsController> logger,
        IYearGroupService yearGroupService) : base(problemFactory, logger)
    {
        _yearGroupService = yearGroupService;
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] Guid academicYearId,
        [FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _yearGroupService.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }

    [HttpGet("{yearGroupId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    public async Task<IActionResult> GetDetailsByIdAsync([FromRoute] Guid yearGroupId)
    {
        var result = await _yearGroupService.GetDetailsByIdAsync(yearGroupId, CancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> CreateAsync([FromBody] YearGroupUpsertRequest model)
    {
        var id = await _yearGroupService.CreateYearGroupAsync(model, CancellationToken);

        return Ok(new { id });
    }

    [HttpPut("{yearGroupId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid yearGroupId,
        [FromBody] YearGroupUpsertRequest model)
    {
        await _yearGroupService.UpdateYearGroupAsync(yearGroupId, model, CancellationToken);

        return NoContent();
    }

    [HttpDelete("{yearGroupId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid yearGroupId)
    {
        await _yearGroupService.DeleteYearGroupAsync(yearGroupId, CancellationToken);

        return NoContent();
    }
}
