using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.School;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>Bulletin endpoints.</summary>
public sealed class BulletinsController(
    ProblemDetailsFactory problemFactory,
    ILogger<BulletinsController> logger,
    IDirectoryEntityService<Bulletin> directoryEntityService,
    IBulletinService bulletinService)
    : BaseDirectoryEntityController<Bulletin>(problemFactory, logger, directoryEntityService)
{
    // Bulletins hard-delete, so their attachments should too.
    protected override bool HardDeleteDocuments => true;

    /// <summary>Get a bulletin by id.</summary>
    /// <param name="bulletinId">The id of the bulletin.</param>
    [HttpGet("{bulletinId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(BulletinDetailsResponse), 200)]
    public async Task<IActionResult> GetBulletinDetailsByIdAsync([FromRoute] Guid bulletinId)
    {
        var result = await bulletinService.GetDetailsByIdAsync(bulletinId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Page through bulletin summaries.</summary>
    /// <remarks>Audience filtering happens server-side. Page size is clamped.</remarks>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(PageResult<BulletinSummaryResponse>), 200)]
    public async Task<IActionResult> GetBulletinsAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await bulletinService.GetBulletinSummariesAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a bulletin.</summary>
    /// <remarks>Posting a pinned bulletin additionally requires <c>School.PinSchoolBulletins</c>.</remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateBulletinAsync([FromBody] BulletinUpsertRequest model)
    {
        var id = await bulletinService.CreateAsync(model, CancellationToken);

        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a bulletin's content and metadata.</summary>
    /// <remarks>Toggling the pin state additionally requires <c>School.PinSchoolBulletins</c>.</remarks>
    /// <param name="bulletinId">The id of the bulletin to update.</param>
    /// <param name="model">The updated content.</param>
    [HttpPut("{bulletinId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateBulletinAsync([FromRoute] Guid bulletinId,
        [FromBody] BulletinUpsertRequest model)
    {
        await bulletinService.UpdateAsync(bulletinId, model, CancellationToken);

        return NoContent();
    }

    /// <summary>Pin or unpin a bulletin.</summary>
    /// <remarks>Uses optimistic concurrency on <c>ExpectedVersion</c>.</remarks>
    /// <param name="bulletinId">The id of the bulletin.</param>
    /// <param name="model">Pin flag and the version the caller last saw.</param>
    [HttpPut("{bulletinId:guid}/pin")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAll, Permissions.School.PinSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> PinBulletinAsync([FromRoute] Guid bulletinId,
        [FromBody] BulletinPinRequest model)
    {
        await bulletinService.UpdatePinAsync(bulletinId, model.IsPinned, model.ExpectedVersion, CancellationToken);

        return NoContent();
    }

    /// <summary>Record the current user's acknowledgement of a bulletin.</summary>
    /// <remarks>Idempotent. Returns 404 if the bulletin is not visible and 400 if it does not require acknowledgement.</remarks>
    /// <param name="bulletinId">The id of the bulletin to acknowledge.</param>
    [HttpPost("{bulletinId:guid}/acknowledge")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> AcknowledgeBulletinAsync([FromRoute] Guid bulletinId)
    {
        await bulletinService.AcknowledgeAsync(bulletinId, CancellationToken);

        return NoContent();
    }

    /// <summary>Delete a bulletin and its attachments.</summary>
    /// <param name="bulletinId">The id of the bulletin to delete.</param>
    [HttpDelete("{bulletinId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteBulletinAsync([FromRoute] Guid bulletinId)
    {
        await bulletinService.DeleteAsync(bulletinId, CancellationToken);

        return NoContent();
    }
}
