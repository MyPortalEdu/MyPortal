using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.WebApi.Controllers;

public class DocumentTypesController : BaseApiController<DocumentTypesController>
{
    private readonly IDocumentService _documentService;

    public DocumentTypesController(ProblemDetailsFactory problemFactory, ILogger<DocumentTypesController> logger,
        IDocumentService documentService) : base(problemFactory, logger)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocumentTypes([FromQuery] DocumentTypeFilter filter)
    {
        var result = await _documentService.GetDocumentTypesAsync(filter, CancellationToken);
        
        return Ok(result);
    }
}