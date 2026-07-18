using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.WebApi.Controllers;

/// <summary>Read-only attendance-code catalogue.</summary>
public sealed class AttendanceCodesController(
    ProblemDetailsFactory problemFactory,
    ILogger<AttendanceCodesController> logger,
    IAttendanceCodeService attendanceCodeService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>List active attendance codes.</summary>
    /// <remarks>Restricted codes are included; inactive codes are excluded.</remarks>
    [HttpGet("active")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(IList<AttendanceCodeResponse>), 200)]
    public async Task<IActionResult> GetActiveAsync()
    {
        var result = await attendanceCodeService.GetActiveAsync(CancellationToken);

        return Ok(result);
    }
}
