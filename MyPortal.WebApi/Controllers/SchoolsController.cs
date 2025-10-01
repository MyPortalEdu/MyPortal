using Microsoft.AspNetCore.Mvc;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.WebApi.Controllers;

public class SchoolsController : BaseApiController
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(IValidationService validationService, ISchoolService schoolService) : base(
        validationService)
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