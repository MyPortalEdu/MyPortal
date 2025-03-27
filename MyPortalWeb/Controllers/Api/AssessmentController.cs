using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Requests.Assessment;

namespace MyPortalWeb.Controllers.Api;

[Microsoft.AspNetCore.Components.Route("api/assessment")]
public sealed class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;

    public AssessmentController(IAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    [HttpGet]
    [Route("results/history")]
    [Permission(PermissionValue.AssessmentViewResults)]
    public async Task<IActionResult> GetPreviousResults([FromQuery] ResultHistoryRequestModel model)
    {
        var resultHistory =
            await _assessmentService.GetPreviousResults(model.StudentId, model.AspectId, model.DateTo);

        return Ok(resultHistory);
    }

    [HttpPut]
    [Route("results")]
    [Permission(PermissionValue.AssessmentEditResults)]
    public async Task<IActionResult> SaveResults([FromBody] UpdateResultsRequestModel model)
    {
        await _assessmentService.SaveResults(model.Results);

        return Ok();
    }
}