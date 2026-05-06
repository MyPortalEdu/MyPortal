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

public sealed class HousesController : BaseApiController<HousesController>
{
    private readonly IHouseService _houseService;

    public HousesController(ProblemDetailsFactory problemFactory, ILogger<HousesController> logger,
        IHouseService houseService) : base(problemFactory, logger)
    {
        _houseService = houseService;
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] Guid academicYearId,
        [FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _houseService.GetSummariesAsync(academicYearId, options.FilterOptions,
            options.SortOptions, options.PageOptions, CancellationToken);

        return Ok(result);
    }

    [HttpGet("{houseId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewPastoralStructure)]
    public async Task<IActionResult> GetDetailsByIdAsync([FromRoute] Guid houseId)
    {
        var result = await _houseService.GetDetailsByIdAsync(houseId, CancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> CreateAsync([FromBody] HouseUpsertRequest model)
    {
        var id = await _houseService.CreateHouseAsync(model, CancellationToken);

        return Ok(new { id });
    }

    [HttpPut("{houseId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid houseId, [FromBody] HouseUpsertRequest model)
    {
        await _houseService.UpdateHouseAsync(houseId, model, CancellationToken);

        return NoContent();
    }

    [HttpDelete("{houseId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditPastoralStructure)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid houseId)
    {
        await _houseService.DeleteHouseAsync(houseId, CancellationToken);

        return NoContent();
    }
}
