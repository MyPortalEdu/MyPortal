using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// School Workforce Census (2026): preview the return assembled from the live personnel data and
/// generate the DfE XML. HR only.
/// </summary>
public sealed class SwfCensusController(
    ProblemDetailsFactory problemFactory,
    ILogger<SwfCensusController> logger,
    ISwfCensusService swfCensusService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Preview the return: header, members and readiness issues.</summary>
    /// <param name="referenceDate">Census reference date. Defaults to 2026-11-05.</param>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(SwfCensusPreviewResponse), 200)]
    public async Task<IActionResult> GetPreviewAsync([FromQuery] DateTime? referenceDate = null)
    {
        var result = await swfCensusService.GetPreviewAsync(referenceDate, CancellationToken);
        return Ok(result);
    }

    /// <summary>Download the School Workforce Census XML.</summary>
    [HttpGet("xml")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(FileResult), 200)]
    public async Task<IActionResult> GetXmlAsync([FromQuery] DateTime? referenceDate = null)
    {
        var xml = await swfCensusService.GenerateXmlAsync(referenceDate, CancellationToken);
        var bytes = Encoding.UTF8.GetBytes(xml);
        return File(bytes, "application/xml", "school-workforce-census-2026.xml");
    }
}
