using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Contracts.Models;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces.Documents;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Read-only access to the document type catalogue. Used by attachment UI to
/// populate the type dropdown when uploading a file.
/// </summary>
public sealed class DocumentTypesController : BaseApiController<DocumentTypesController>
{
    private readonly IDocumentService _documentService;

    public DocumentTypesController(ProblemDetailsFactory problemFactory, ILogger<DocumentTypesController> logger,
        IDocumentService documentService) : base(problemFactory, logger)
    {
        _documentService = documentService;
    }

    /// <summary>List document types matching the given filter.</summary>
    /// <param name="filter">Optional filtering criteria (active flag, content-type prefix, etc.).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IList<LookupResponse>), 200)]
    public async Task<IActionResult> GetDocumentTypes([FromQuery] DocumentTypeFilter filter)
    {
        var result = await _documentService.GetDocumentTypesAsync(filter, CancellationToken);

        return Ok(result);
    }
}