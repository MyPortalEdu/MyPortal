using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.WebApi.Infrastructure.Attributes;
using MyPortal.WebApi.Models.Documents;

namespace MyPortal.WebApi.Controllers;


/// <summary>
/// Base controller for any entity that owns an attachments tree (directories +
/// documents). Endpoints under <c>/{entityId}/attachments/...</c> are inherited
/// by every concrete subclass (e.g. <see cref="BulletinsController"/>) and act
/// against the entity identified by <c>entityId</c> in the route.
/// </summary>
public abstract class BaseDirectoryEntityController<TDirectoryEntity> : BaseApiController where TDirectoryEntity : IDirectoryEntity
{
    private readonly IDirectoryEntityService<TDirectoryEntity> _directoryEntityService;

    protected BaseDirectoryEntityController(ProblemDetailsFactory problemFactory, ILogger<BaseDirectoryEntityController<TDirectoryEntity>> logger,
        IDirectoryEntityService<TDirectoryEntity> directoryEntityService) : base(problemFactory, logger)
    {
        _directoryEntityService = directoryEntityService;
    }

    /// <summary>
    /// Whether DELETE on attachments under this entity should hard-delete
    /// (true) or soft-delete (false, the default). Subclasses whose
    /// attachments have no retention requirement and would otherwise pile
    /// up as soft-deleted rows (bulletins, future news/announcements)
    /// override this. Subclasses owning person-scoped documents (student,
    /// staff, contact) should leave it as the default so deletions remain
    /// recoverable / auditable.
    /// </summary>
    protected virtual bool HardDeleteDocuments => false;

    /// <summary>Get a single directory by id.</summary>
    /// <param name="entityId">The owning entity (bulletin, etc.).</param>
    /// <param name="directoryId">The directory to fetch.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    [ProducesResponseType(typeof(DirectoryDetailsResponse), 200)]
    public async Task<IActionResult> GetDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryByIdAsync(entityId, directoryId, CancellationToken);

        return Ok(result);
    }

    /// <summary>List the immediate contents (sub-directories + documents) of a directory.</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The directory whose contents to list.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}/contents")]
    [ProducesResponseType(typeof(DirectoryContentsResponse), 200)]
    public async Task<IActionResult> GetDirectoryContentsAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryContentsAsync(entityId, directoryId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the full directory tree rooted at a directory.</summary>
    /// <remarks>
    /// Returns all sub-directories recursively. Used by the SPA's tree-picker UI
    /// when moving items between directories.
    /// </remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The root directory to start the tree at.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}/tree")]
    [ProducesResponseType(typeof(DirectoryTreeResponse), 200)]
    public async Task<IActionResult> GetDirectoryTreeAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryTreeAsync(entityId, directoryId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a new directory under the entity's root.</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="model">The new directory's name and parent.</param>
    [HttpPost("{entityId:guid}/attachments/directories")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(DirectoryDetailsResponse), 200)]
    public async Task<IActionResult> CreateDirectoryAsync([FromRoute] Guid entityId,
        [FromBody] DirectoryUpsertRequest model)
    {
        var result = await _directoryEntityService.CreateDirectoryAsync(entityId, model, CancellationToken);

        return Ok(result);
    }

    /// <summary>Rename or move a directory.</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The directory to update.</param>
    /// <param name="model">Updated name and/or parent.</param>
    [HttpPut("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(DirectoryDetailsResponse), 200)]
    public async Task<IActionResult> UpdateDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId,
        [FromBody] DirectoryUpsertRequest model)
    {
        var result = await _directoryEntityService.UpdateDirectoryAsync(entityId, directoryId, model, CancellationToken);

        return Ok(result);
    }

    /// <summary>Delete a directory and its contents.</summary>
    /// <remarks>Recursively deletes sub-directories and documents. Cannot be undone — backend may reject if soft-delete is configured.</remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The directory to delete.</param>
    [HttpDelete("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        await _directoryEntityService.DeleteDirectoryAsync(entityId, directoryId, CancellationToken, softDelete: !HardDeleteDocuments);

        return NoContent();
    }

    /// <summary>Get a document's metadata (filename, content type, size, hash, etc.).</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document whose metadata to fetch.</param>
    [HttpGet("{entityId:guid}/attachments/documents/{documentId:guid}")]
    [ProducesResponseType(typeof(DocumentDetailsResponse), 200)]
    public async Task<IActionResult> GetDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        var result = await _directoryEntityService.GetDocumentByIdAsync(entityId, documentId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Download a document's content.</summary>
    /// <remarks>
    /// Returns the file body with sanitised <c>Content-Type</c>, <c>X-Content-Type-Options: nosniff</c>,
    /// and an <c>ETag</c>/<c>Last-Modified</c> when available so the browser can cache.
    /// Supports range requests on seekable streams.
    /// </remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document to download.</param>
    [HttpGet("{entityId:guid}/attachments/documents/{documentId:guid}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    public async Task<IActionResult> DownloadDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        var document =
            await _directoryEntityService.GetDocumentWithContentByIdAsync(entityId, documentId, CancellationToken);

        var typedHeaders = Response.GetTypedHeaders();

        if (!string.IsNullOrWhiteSpace(document.Details.Hash))
        {
            typedHeaders.ETag = new EntityTagHeaderValue($"\"{document.Details.Hash}\"");
        }

        typedHeaders.LastModified = document.Details.LastModifiedAt;

        Response.Headers["X-Content-Type-Options"] = "nosniff";

        var safeContentType = SafeContentTypes.Sanitize(document.Details.ContentType);

        return File(document.Content, safeContentType, document.Details.FileName,
            enableRangeProcessing: document.Content.CanSeek);
    }

    /// <summary>Upload a new document to the entity.</summary>
    /// <remarks>
    /// Multipart form upload. <c>File</c> is the binary content; the rest of the
    /// form populates document metadata (type, target directory, title,
    /// description, privacy flag).
    /// </remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="form">The file plus metadata fields.</param>
    [HttpPost("{entityId:guid}/attachments/documents")]
    [ValidateModel]
    // No explicit size attributes: we sit on Kestrel's default
    // MaxRequestBodySize (30,000,000 bytes) as the server-side ceiling, and
    // the SPA enforces the same cap up-front (MAX_ATTACHMENT_BYTES in
    // shared/types/document.ts) so oversize files toast immediately instead
    // of resetting the connection.
    [ProducesResponseType(typeof(DocumentDetailsResponse), 200)]
    public async Task<IActionResult> CreateDocumentAsync([FromRoute] Guid entityId,
        [FromForm] DocumentUpsertForm form)
    {
        var model = new DocumentUpsertRequest
        {
            TypeId = form.TypeId,
            DirectoryId = form.DirectoryId,
            Title = form.Title,
            Description = form.Description,
            IsPrivate = form.IsPrivate
        };

        await using var stream = form.File?.OpenReadStream();

        if (form.File != null)
        {
            model.FileName = form.File.FileName;
            model.Content = stream;
            model.ContentType = form.File.ContentType;
            model.SizeBytes = form.File.Length;
        }
        
        var result = await _directoryEntityService.CreateDocumentAsync(entityId, model, CancellationToken);
        
        return Ok(result);
    }

    /// <summary>Update a document's metadata and/or replace its content.</summary>
    /// <remarks>
    /// If <c>File</c> is omitted from the form, only metadata is updated; the
    /// existing binary content is preserved. If <c>File</c> is present, the
    /// content is replaced and a new hash recorded.
    /// </remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document to update.</param>
    /// <param name="form">Metadata and (optionally) replacement file.</param>
    [HttpPut("{entityId:guid}/attachments/documents/{documentId:guid}")]
    [ValidateModel]
    [ProducesResponseType(typeof(DocumentDetailsResponse), 200)]
    public async Task<IActionResult> UpdateDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId,
        [FromForm] DocumentUpsertForm form)
    {
        var model = new DocumentUpsertRequest
        {
            TypeId = form.TypeId,
            DirectoryId = form.DirectoryId,
            Title = form.Title,
            Description = form.Description,
            IsPrivate = form.IsPrivate
        };

        await using var stream = form.File?.OpenReadStream();

        if (form.File != null)
        {
            model.FileName = form.File.FileName;
            model.Content = stream;
            model.ContentType = form.File.ContentType;
            model.SizeBytes = form.File.Length;
        }

        var result = await _directoryEntityService.UpdateDocumentAsync(entityId, documentId, model, CancellationToken);
        
        return Ok(result);
    }

    /// <summary>Delete a document.</summary>
    /// <remarks>
    /// Backed by either a hard delete or a soft delete depending on the storage
    /// provider. The blob in file storage is removed on a hard delete.
    /// </remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document to delete.</param>
    [HttpDelete("{entityId:guid}/attachments/documents/{documentId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        await _directoryEntityService.DeleteDocumentAsync(entityId, documentId, CancellationToken,
            softDelete: !HardDeleteDocuments);

        return NoContent();
    }
}