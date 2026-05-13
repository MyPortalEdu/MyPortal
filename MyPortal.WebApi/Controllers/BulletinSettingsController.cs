using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Interfaces.School;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage school-wide bulletin settings, currently the allowlist of student
/// groups that may be picked as a bulletin audience. Read access is granted
/// alongside View permission; mutating requires the bulletin settings permission.
/// </summary>
[Route("api/bulletins/settings")]
public sealed class BulletinSettingsController : BaseApiController
{
    private readonly IBulletinSettingsService _service;

    public BulletinSettingsController(ProblemDetailsFactory problemFactory,
        ILogger<BulletinSettingsController> logger, IBulletinSettingsService service)
        : base(problemFactory, logger)
    {
        _service = service;
    }

    /// <summary>Get current bulletin settings.</summary>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(BulletinSettingsResponse), 200)]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Replace the allowlist of bulletin-audience-pickable student groups.</summary>
    /// <remarks>Admin-tier — gated on <c>System.BulletinSettings</c>.</remarks>
    [HttpPut]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.SystemAdmin.BulletinSettings)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateAsync([FromBody] BulletinSettingsUpdateRequest model)
    {
        await _service.UpdateAsync(model, CancellationToken);
        return NoContent();
    }
}
