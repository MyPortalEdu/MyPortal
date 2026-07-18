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


/// <summary>Shared attachment endpoints for directory-owning entities.</summary>
public abstract class BaseDirectoryEntityController<TDirectoryEntity>(
    ProblemDetailsFactory problemFactory,
    ILogger<BaseDirectoryEntityController<TDirectoryEntity>> logger,
    IDirectoryEntityService<TDirectoryEntity> directoryEntityService)
    : BaseApiController(problemFactory, logger)
    where TDirectoryEntity : IDirectoryEntity
{
    /// <summary>Whether attachment deletes should hard-delete instead of soft-delete.</summary>
    protected virtual bool HardDeleteDocuments => false;

    /// <summary>Get a single directory by id.</summary>
    /// <param name="entityId">The owning entity (bulletin, etc.).</param>
    /// <param name="directoryId">The directory to fetch.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    [ProducesResponseType(typeof(DirectoryDetailsResponse), 200)]
    public async Task<IActionResult> GetDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await directoryEntityService.GetDirectoryByIdAsync(entityId, directoryId, CancellationToken);

        return Ok(result);
    }

    /// <summary>List the immediate contents (sub-directories + documents) of a directory.</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The directory whose contents to list.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}/contents")]
    [ProducesResponseType(typeof(DirectoryContentsResponse), 200)]
    public async Task<IActionResult> GetDirectoryContentsAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await directoryEntityService.GetDirectoryContentsAsync(entityId, directoryId, CancellationToken);

        return Ok(result);
    }

    /// <summary>List the immediate contents of the entity's root directory.</summary>
    /// <remarks>Use this when the caller knows the entity id but not the root directory id.</remarks>
    /// <param name="entityId">The owning entity.</param>
    [HttpGet("{entityId:guid}/attachments/root/contents")]
    [ProducesResponseType(typeof(DirectoryContentsResponse), 200)]
    public async Task<IActionResult> GetRootDirectoryContentsAsync([FromRoute] Guid entityId)
    {
        var result = await directoryEntityService.GetRootDirectoryContentsAsync(entityId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the full directory tree rooted at a directory.</summary>
    /// <remarks>Returns the full subtree for tree-picker style UIs.</remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The root directory to start the tree at.</param>
    [HttpGet("{entityId:guid}/attachments/directories/{directoryId:guid}/tree")]
    [ProducesResponseType(typeof(DirectoryTreeResponse), 200)]
    public async Task<IActionResult> GetDirectoryTreeAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await directoryEntityService.GetDirectoryTreeAsync(entityId, directoryId, CancellationToken);

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
        var result = await directoryEntityService.CreateDirectoryAsync(entityId, model, CancellationToken);

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
        var result = await directoryEntityService.UpdateDirectoryAsync(entityId, directoryId, model, CancellationToken);

        return Ok(result);
    }

    /// <summary>Delete a directory and its contents.</summary>
    /// <remarks>Recursively deletes sub-directories and documents.</remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="directoryId">The directory to delete.</param>
    [HttpDelete("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        await directoryEntityService.DeleteDirectoryAsync(entityId, directoryId, CancellationToken, softDelete: !HardDeleteDocuments);

        return NoContent();
    }

    /// <summary>Get a document's metadata (filename, content type, size, hash, etc.).</summary>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document whose metadata to fetch.</param>
    [HttpGet("{entityId:guid}/attachments/documents/{documentId:guid}")]
    [ProducesResponseType(typeof(DocumentDetailsResponse), 200)]
    public async Task<IActionResult> GetDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        var result = await directoryEntityService.GetDocumentByIdAsync(entityId, documentId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Download a document's content.</summary>
    /// <remarks>Returns the file body with safe headers and range support where available.</remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document to download.</param>
    [HttpGet("{entityId:guid}/attachments/documents/{documentId:guid}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    public async Task<IActionResult> DownloadDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        var document =
            await directoryEntityService.GetDocumentWithContentByIdAsync(entityId, documentId, CancellationToken);

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
    /// <remarks>Multipart form upload with file content and document metadata.</remarks>
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
        
        var result = await directoryEntityService.CreateDocumentAsync(entityId, model, CancellationToken);
        
        return Ok(result);
    }

    /// <summary>Update a document's metadata and/or replace its content.</summary>
    /// <remarks>If <c>File</c> is omitted, only metadata is updated.</remarks>
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

        var result = await directoryEntityService.UpdateDocumentAsync(entityId, documentId, model, CancellationToken);
        
        return Ok(result);
    }

    /// <summary>Delete a document.</summary>
    /// <remarks>Uses hard or soft delete depending on the entity configuration.</remarks>
    /// <param name="entityId">The owning entity.</param>
    /// <param name="documentId">The document to delete.</param>
    [HttpDelete("{entityId:guid}/attachments/documents/{documentId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        await directoryEntityService.DeleteDocumentAsync(entityId, documentId, CancellationToken,
            softDelete: !HardDeleteDocuments);

        return NoContent();
    }
}
