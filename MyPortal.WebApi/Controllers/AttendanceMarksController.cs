using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Services.Interfaces.Attendance;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

public sealed class AttendanceMarksController : BaseApiController<AttendanceMarksController>
{
    private readonly IAttendanceMarkService _attendanceMarkService;

    public AttendanceMarksController(ProblemDetailsFactory problemFactory,
        ILogger<AttendanceMarksController> logger, IAttendanceMarkService attendanceMarkService)
        : base(problemFactory, logger)
    {
        _attendanceMarkService = attendanceMarkService;
    }

    [HttpGet("bulk")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    public async Task<IActionResult> GetBulkAsync([FromQuery] Guid studentGroupId,
        [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _attendanceMarkService.GetBulkAsync(studentGroupId, from, to, CancellationToken);

        return Ok(result);
    }

    [HttpPut("bulk")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarksBulk)]
    public async Task<IActionResult> SubmitBulkAsync([FromBody] BulkAttendanceMarksRequest model)
    {
        await _attendanceMarkService.SubmitBulkAsync(model, CancellationToken);

        return NoContent();
    }
}
