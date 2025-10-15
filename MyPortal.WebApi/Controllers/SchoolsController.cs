using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.WebApi.Controllers;

public class SchoolsController : BaseApiController<SchoolsController>
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(ProblemDetailsFactory problemFactory, ILogger<SchoolsController> logger,
        ISchoolService schoolService) : base(problemFactory, logger)
    {
        _schoolService = schoolService;
    }

    [HttpGet("local/name")]
    public async Task<IActionResult> GetLocalSchoolName()
    {
        var school = await _schoolService.GetLocalSchoolAsync(CancellationToken);
        
        return Ok(school?.Name ?? "");
    }
}