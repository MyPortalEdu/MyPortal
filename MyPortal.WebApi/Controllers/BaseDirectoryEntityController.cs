using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Models.Documents;

namespace MyPortal.WebApi.Controllers;


public abstract class BaseDirectoryEntityController<TSelf> : BaseApiController<TSelf>
{
    private readonly IDirectoryEntityService _directoryEntityService;

    public BaseDirectoryEntityController(ProblemDetailsFactory problemFactory, ILogger<TSelf> logger,
        IDirectoryEntityService directoryEntityService) : base(problemFactory, logger)
    {
        _directoryEntityService = directoryEntityService;
    }

    [HttpGet]
    [Route("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    public async Task<IActionResult> GetDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryByIdAsync(entityId, directoryId, CancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    [Route("{entityId:guid}/attachments/directories/{directoryId:guid}/contents")]
    public async Task<IActionResult> GetDirectoryContentsAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryContentsAsync(entityId, directoryId, CancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    [Route("{entityId:guid}/attachments/directories/{directoryId:guid}/tree")]
    public async Task<IActionResult> GetDirectoryTreeAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        var result = await _directoryEntityService.GetDirectoryTreeAsync(entityId, directoryId, CancellationToken);
        
        return Ok(result);
    }

    [HttpPost]
    [Route("{entityId:guid}/attachments/directories")]
    public async Task<IActionResult> CreateDirectoryAsync([FromRoute] Guid entityId,
        [FromBody] DirectoryUpsertRequest model)
    {
        var result = await _directoryEntityService.CreateDirectoryAsync(entityId, model, CancellationToken);
        
        return Ok(result);
    }

    [HttpPut]
    [Route("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    public async Task<IActionResult> UpdateDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId,
        [FromBody] DirectoryUpsertRequest model)
    {
        var result = await _directoryEntityService.UpdateDirectoryAsync(entityId, directoryId, model, CancellationToken);
        
        return Ok(result);
    }

    [HttpDelete]
    [Route("{entityId:guid}/attachments/directories/{directoryId:guid}")]
    public async Task<IActionResult> DeleteDirectoryAsync([FromRoute] Guid entityId, [FromRoute] Guid directoryId)
    {
        await _directoryEntityService.DeleteDirectoryAsync(entityId, directoryId, CancellationToken);

        return NoContent();
    }

    [HttpGet]
    [Route("{entityId:guid}/attachments/{documentId:guid}")]
    public async Task<IActionResult> GetDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        var result = await _directoryEntityService.GetDocumentByIdAsync(entityId, documentId, CancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    [Route("{entityId:guid}/attachments/{documentId:guid}/download")]
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

        return File(document.Content, document.Details.ContentType, document.Details.FileName,
            enableRangeProcessing: document.Content.CanSeek);
    }

    [HttpPost]
    [Route("{entityId:guid}/attachments/documents")]
    public async Task<IActionResult> CreateDocumentAsync([FromRoute] Guid entityId,
        [FromBody] DocumentUpsertForm form)
    {
        await using var stream = form.File.OpenReadStream();

        var model = new DocumentUpsertRequest
        {
            TypeId = form.TypeId,
            DirectoryId = form.DirectoryId,
            Title = form.Title,
            Description = form.Description,
            IsPrivate = form.IsPrivate
        };
        
        model.FileName = form.File.FileName;
        model.Content = stream;
        model.ContentType = form.File.ContentType;
        model.SizeBytes = form.File.Length;
        
        var result = await _directoryEntityService.CreateDocumentAsync(entityId, model, CancellationToken);
        
        return Ok(result);
    }

    [HttpPut]
    [Route("{entityId:guid}/attachments/documents/{documentId:guid}")]
    public async Task<IActionResult> UpdateDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId,
        [FromBody] DocumentUpsertForm form)
    {
        await using var stream = form.File.OpenReadStream();

        var model = new DocumentUpsertRequest
        {
            TypeId = form.TypeId,
            DirectoryId = form.DirectoryId,
            Title = form.Title,
            Description = form.Description,
            IsPrivate = form.IsPrivate
        };

        model.FileName = form.File.FileName;
        model.Content = stream;
        model.ContentType = form.File.ContentType;
        model.SizeBytes = form.File.Length;

        var result = await _directoryEntityService.UpdateDocumentAsync(entityId, documentId, model, CancellationToken);
        
        return Ok(result);
    }

    [HttpDelete]
    [Route("{entityId:guid}/attachments/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocumentAsync([FromRoute] Guid entityId, [FromRoute] Guid documentId)
    {
        await _directoryEntityService.DeleteDocumentAsync(entityId, documentId, CancellationToken);
        
        return NoContent();
    }
}