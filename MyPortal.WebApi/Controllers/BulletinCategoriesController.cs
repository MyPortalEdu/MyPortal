using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Interfaces.School;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage the bulletin category taxonomy (name, icon, colour). Categories are
/// user-managed — defaults are seeded but can be renamed, recoloured, deactivated,
/// or replaced.
/// </summary>
public sealed class BulletinCategoriesController : BaseApiController
{
    private readonly IBulletinCategoryService _service;

    public BulletinCategoriesController(ProblemDetailsFactory problemFactory,
        ILogger<BulletinCategoriesController> logger, IBulletinCategoryService service)
        : base(problemFactory, logger)
    {
        _service = service;
    }

    /// <summary>List bulletin categories.</summary>
    /// <param name="includeInactive">Include categories with Active=false. Defaults to false.</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(IList<BulletinCategoryResponse>), 200)]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single bulletin category by id.</summary>
    [HttpGet("{categoryId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(BulletinCategoryResponse), 200)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid categoryId)
    {
        var result = await _service.GetByIdAsync(categoryId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new bulletin category.</summary>
    /// <remarks>Admin-tier — gated on <c>System.BulletinSettings</c>.</remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.SystemAdmin.BulletinSettings)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] BulletinCategoryUpsertRequest model)
    {
        var id = await _service.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a bulletin category.</summary>
    /// <remarks>Admin-tier — gated on <c>System.BulletinSettings</c>. Uses optimistic concurrency on <c>ExpectedVersion</c>.</remarks>
    [HttpPut("{categoryId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.SystemAdmin.BulletinSettings)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid categoryId,
        [FromBody] BulletinCategoryUpsertRequest model)
    {
        await _service.UpdateAsync(categoryId, model, CancellationToken);
        return NoContent();
    }

    /// <summary>Delete a bulletin category.</summary>
    /// <remarks>Admin-tier — gated on <c>System.BulletinSettings</c>. Fails if any bulletin still references the category.</remarks>
    [HttpDelete("{categoryId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.SystemAdmin.BulletinSettings)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid categoryId)
    {
        await _service.DeleteAsync(categoryId, CancellationToken);
        return NoContent();
    }
}
