using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

public class BulletinsController : BaseApiController<BulletinsController>
{
    private readonly IBulletinService _bulletinService;

    public BulletinsController(ProblemDetailsFactory problemFactory, ILogger<BulletinsController> logger,
        IBulletinService bulletinService) : base(problemFactory, logger)
    {
        _bulletinService = bulletinService;
    }

    [HttpGet("bulletinId:guid")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins,
        Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> GetBulletinDetailsByIdAsync([FromRoute] Guid bulletinId)
    {
        var result = await _bulletinService.GetDetailsByIdAsync(bulletinId, CancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins,
        Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> GetBulletinsAsync([FromQuery] FilterOptions? filter = null,
        [FromQuery] SortOptions? sort = null, [FromQuery] PageOptions? paging = null)
    {
        var result = await _bulletinService.GetBulletinsAsync(filter, sort, paging, CancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Permission(PermissionMode.RequireAll, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> CreateBulletinAsync([FromBody] BulletinUpsertDto model)
    {
        var result = await _bulletinService.CreateBulletinAsync(model, CancellationToken);
        
        return Ok(result);
    }

    [HttpPut("bulletinId:guid")]
    [Permission(PermissionMode.RequireAll, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> UpdateBulletinAsync([FromRoute] Guid bulletinId, [FromBody] BulletinUpsertDto model)
    {
        await _bulletinService.UpdateBulletinAsync(bulletinId, model, CancellationToken);

        return NoContent();
    }

    [HttpPut("bulletinId:guid/approve")]
    [Permission(PermissionMode.RequireAll, Permissions.School.ApproveSchoolBulletins)]
    public async Task<IActionResult> ApproveBulletinAsync([FromRoute] Guid bulletinId,
        [FromBody] BulletinApprovalDto model)
    {
        await _bulletinService.UpdateBulletinApprovalAsync(bulletinId, model.IsApproved, CancellationToken);

        return NoContent();
    }

    [HttpDelete("bulletinId:guid")]
    [Permission(PermissionMode.RequireAll, Permissions.School.EditSchoolBulletins)]
    public async Task<IActionResult> DeleteBulletinAsync([FromRoute] Guid bulletinId)
    {
        await _bulletinService.DeleteBulletinAsync(bulletinId, CancellationToken);
        
        return NoContent();
    }
}