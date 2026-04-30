using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

public sealed class BulletinsController : BaseDirectoryEntityController<BulletinsController, Bulletin>
{
    private readonly IBulletinService _bulletinService;

    public BulletinsController(ProblemDetailsFactory problemFactory, ILogger<BulletinsController> logger,
        IDirectoryEntityService<Bulletin> directoryEntityService, IBulletinService bulletinService) : base(
        problemFactory, logger, directoryEntityService)
    {
        _bulletinService = bulletinService;
    }

    [HttpGet("{bulletinId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    public async Task<IActionResult> GetBulletinDetailsByIdAsync([FromRoute] Guid bulletinId)
    {
        var result = await _bulletinService.GetDetailsByIdAsync(bulletinId, CancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    public async Task<IActionResult> GetBulletinsAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _bulletinService.GetBulletinsAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> CreateBulletinAsync([FromBody] BulletinUpsertRequest model)
    {
        var result = await _bulletinService.CreateBulletinAsync(model, CancellationToken);

        return Ok(result);
    }

    [HttpPut("{bulletinId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> UpdateBulletinAsync([FromRoute] Guid bulletinId,
        [FromBody] BulletinUpsertRequest model)
    {
        await _bulletinService.UpdateBulletinAsync(bulletinId, model, CancellationToken);

        return NoContent();
    }

    [HttpPut("{bulletinId:guid}/approve")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.School.ApproveSchoolBulletins)]
    public async Task<IActionResult> ApproveBulletinAsync([FromRoute] Guid bulletinId,
        [FromBody] BulletinApprovalRequest model)
    {
        await _bulletinService.UpdateBulletinApprovalAsync(bulletinId, model.IsApproved, model.ExpectedVersion,
            CancellationToken);

        return NoContent();
    }

    [HttpDelete("{bulletinId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> DeleteBulletinAsync([FromRoute] Guid bulletinId)
    {
        await _bulletinService.DeleteBulletinAsync(bulletinId, CancellationToken);

        return NoContent();
    }
}