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

/// <summary>Bulletin settings endpoints.</summary>
// Explicit Route overrides BaseApiController's "api/[controller]" pair.
// Mirrors the dual-route pattern on the base: versioned canonical, unversioned alias.
[Route("api/bulletins/settings")]
[Route("api/v{version:apiVersion}/bulletins/settings")]
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

    /// <summary>Replace the bulletin audience allowlist.</summary>
    /// <remarks>Admin-tier; gated on <c>System.BulletinSettings</c>.</remarks>
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
