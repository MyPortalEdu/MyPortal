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

/// <summary>
/// Manage school bulletins — short news items broadcast to staff, pupils, parents,
/// or specific student groups via the audience model. Pinning is admin-only;
/// authorship gives edit/delete on your own bulletins. Inherits attachment
/// endpoints (directories/documents) from
/// <see cref="BaseDirectoryEntityController{TSelf, TDirectoryEntity}"/>.
/// </summary>
public sealed class BulletinsController : BaseDirectoryEntityController<BulletinsController, Bulletin>
{
    private readonly IBulletinService _bulletinService;

    public BulletinsController(ProblemDetailsFactory problemFactory, ILogger<BulletinsController> logger,
        IDirectoryEntityService<Bulletin> directoryEntityService, IBulletinService bulletinService) : base(
        problemFactory, logger, directoryEntityService)
    {
        _bulletinService = bulletinService;
    }

    /// <summary>Get the full details of a bulletin by id.</summary>
    /// <param name="bulletinId">The id of the bulletin.</param>
    [HttpGet("{bulletinId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(typeof(BulletinDetailsResponse), 200)]
    public async Task<IActionResult> GetBulletinDetailsByIdAsync([FromRoute] Guid bulletinId)
    {
        var result = await _bulletinService.GetDetailsByIdAsync(bulletinId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Page through bulletin summaries.</summary>
    /// <remarks>
    /// The set is audience-filtered server-side: pupils see AllPupils + their student
    /// groups, parents see AllParents + their wards' groups, staff see AllStaff
    /// (or everything when they hold the pin permission). Page size is clamped
    /// server-side (default 25, max 100).
    /// </remarks>
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

        var result = await _bulletinService.GetBulletinSummariesAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a new bulletin.</summary>
    /// <remarks>
    /// Posting a pinned bulletin additionally requires the
    /// <c>School.PinSchoolBulletins</c> permission. Audience is required
    /// (the validator rejects an empty list).
    /// </remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateBulletinAsync([FromBody] BulletinUpsertRequest model)
    {
        var id = await _bulletinService.CreateAsync(model, CancellationToken);

        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a bulletin's content/metadata.</summary>
    /// <remarks>
    /// Toggling the pin state additionally requires <c>School.PinSchoolBulletins</c>.
    /// Uses optimistic concurrency on <c>ExpectedVersion</c>.
    /// </remarks>
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
        await _bulletinService.UpdateAsync(bulletinId, model, CancellationToken);

        return NoContent();
    }

    /// <summary>Pin or unpin a bulletin.</summary>
    /// <remarks>
    /// Pinned bulletins sort to the top of feeds. Pinning is an admin-only
    /// gesture independent of authorship. Uses optimistic concurrency on
    /// <c>ExpectedVersion</c>.
    /// </remarks>
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
        await _bulletinService.UpdatePinAsync(bulletinId, model.IsPinned, model.ExpectedVersion, CancellationToken);

        return NoContent();
    }

    /// <summary>Record the current user's acknowledgement of a bulletin.</summary>
    /// <remarks>
    /// Idempotent: re-acknowledging is a no-op. 404 if the bulletin is not
    /// visible to the caller. 400 if the bulletin does not require ack.
    /// </remarks>
    /// <param name="bulletinId">The id of the bulletin to acknowledge.</param>
    [HttpPost("{bulletinId:guid}/acknowledge")]
    [Permission(PermissionMode.RequireAny, Permissions.School.ViewSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> AcknowledgeBulletinAsync([FromRoute] Guid bulletinId)
    {
        await _bulletinService.AcknowledgeAsync(bulletinId, CancellationToken);

        return NoContent();
    }

    /// <summary>Delete a bulletin and any attachments it owns.</summary>
    /// <param name="bulletinId">The id of the bulletin to delete.</param>
    [HttpDelete("{bulletinId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.School.EditSchoolBulletins)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteBulletinAsync([FromRoute] Guid bulletinId)
    {
        await _bulletinService.DeleteAsync(bulletinId, CancellationToken);

        return NoContent();
    }
}
