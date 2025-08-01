using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Database.Models.Filters;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.School;
using MyPortal.Logic.Models.Requests.School.Bulletins;

namespace MyPortalWeb.Controllers.Api;

[Route("api/bulletins")]
public class BulletinsController : ControllerBase
{
    private readonly IBulletinService _bulletinService;

    public BulletinsController(IBulletinService bulletinService)
    {
        _bulletinService = bulletinService;
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(BulletinPageResponse), 200)]
    public async Task<IActionResult> GetSchoolBulletins([FromQuery] BulletinSearchOptions searchOptions,
        [FromQuery] PageFilter filter)
    {
        var bulletins = await _bulletinService.GetBulletinSummaries(searchOptions, filter);

        return Ok(bulletins);
    }

    [HttpPost]
    [Route("")]
    [Authorize(Policies.UserType.Staff)]
    [Permission(PermissionValue.SchoolEditSchoolBulletins)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CreateBulletin([FromBody] BulletinRequestModel model)
    {
        await _bulletinService.CreateBulletin(model);
        return Ok();
    }

    [HttpPost]
    [Route("{bulletinId}")]
    [Authorize(Policies.UserType.Staff)]
    [Permission(PermissionValue.SchoolEditSchoolBulletins)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UpdateBulletin([FromRoute] Guid bulletinId,
        [FromBody] BulletinRequestModel model)
    {
        await _bulletinService.UpdateBulletin(bulletinId, model);
        return Ok();
    }

    [HttpPost]
    [Route("{bulletinId}/approve")]
    [Authorize(Policies.UserType.Staff)]
    [Permission(PermissionValue.SchoolApproveSchoolBulletins)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ApproveBulletin([FromRoute] Guid bulletinId,
        [FromBody] ApproveBulletinRequestModel model)
    {
        model.BulletinId = bulletinId;
        await _bulletinService.SetBulletinApproved(model);
        return Ok();
    }

    [HttpDelete]
    [Route("{bulletinId}")]
    [Authorize(Policies.UserType.Staff)]
    [Permission(PermissionValue.SchoolEditSchoolBulletins)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DeleteBulletin([FromRoute] Guid bulletinId)
    {
        await _bulletinService.DeleteBulletin(bulletinId);
        return Ok();
    }
}