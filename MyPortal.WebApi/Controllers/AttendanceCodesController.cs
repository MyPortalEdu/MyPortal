using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.WebApi.Controllers;

public sealed class AttendanceCodesController : BaseApiController<AttendanceCodesController>
{
    private readonly IAttendanceCodeService _attendanceCodeService;

    public AttendanceCodesController(ProblemDetailsFactory problemFactory,
        ILogger<AttendanceCodesController> logger, IAttendanceCodeService attendanceCodeService)
        : base(problemFactory, logger)
    {
        _attendanceCodeService = attendanceCodeService;
    }

    [HttpGet("active")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    public async Task<IActionResult> GetActiveAsync()
    {
        var result = await _attendanceCodeService.GetActiveAsync(CancellationToken);

        return Ok(result);
    }
}
