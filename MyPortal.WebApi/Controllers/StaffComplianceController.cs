using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// The staff HR compliance dashboard — expiring/expired and missing safeguarding, right-to-work,
/// training and contract records across the school. HR/safeguarding only.
/// </summary>
public sealed class StaffComplianceController(
    ProblemDetailsFactory problemFactory,
    ILogger<StaffComplianceController> logger,
    IStaffComplianceService staffComplianceService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get the compliance dashboard.</summary>
    /// <param name="horizonDays">How far ahead "expiring soon" looks. Defaults to 90; clamped 7–365.</param>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffComplianceDashboardResponse), 200)]
    public async Task<IActionResult> GetDashboardAsync([FromQuery] int horizonDays = 90)
    {
        var result = await staffComplianceService.GetDashboardAsync(horizonDays, CancellationToken);
        return Ok(result);
    }
}
