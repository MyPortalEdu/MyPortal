using Microsoft.AspNetCore.Mvc;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.WebApi.Controllers;

public class SchoolsController : BaseApiController
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(ISchoolService schoolService)
    {
        _schoolService = schoolService;
    }
    
    [HttpGet("local/name")]
    public async Task<IActionResult> GetLocalSchoolName()
    {
        var school = await _schoolService.GetLocalSchool(CancellationToken);
        
        return Ok(school?.Name ?? "");
    }
}