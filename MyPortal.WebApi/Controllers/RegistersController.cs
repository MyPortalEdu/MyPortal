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

public sealed class RegistersController : BaseApiController<RegistersController>
{
    private readonly IRegisterService _registerService;

    public RegistersController(ProblemDetailsFactory problemFactory, ILogger<RegistersController> logger,
        IRegisterService registerService) : base(problemFactory, logger)
    {
        _registerService = registerService;
    }

    [HttpGet("lesson/{sessionPeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    public async Task<IActionResult> GetLessonRegisterAsync([FromRoute] Guid sessionPeriodId,
        [FromRoute] Guid attendanceWeekId)
    {
        var result = await _registerService.GetLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            CancellationToken);

        return Ok(result);
    }

    [HttpGet("reg-group/{regGroupId:guid}/{attendancePeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    public async Task<IActionResult> GetRegGroupRegisterAsync([FromRoute] Guid regGroupId,
        [FromRoute] Guid attendancePeriodId, [FromRoute] Guid attendanceWeekId)
    {
        var result = await _registerService.GetRegGroupRegisterAsync(regGroupId, attendancePeriodId,
            attendanceWeekId, CancellationToken);

        return Ok(result);
    }

    [HttpPut("lesson/{sessionPeriodId:guid}/{attendanceWeekId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarks)]
    public async Task<IActionResult> SubmitLessonRegisterAsync([FromRoute] Guid sessionPeriodId,
        [FromRoute] Guid attendanceWeekId, [FromBody] SubmitRegisterRequest model)
    {
        await _registerService.SubmitLessonRegisterAsync(sessionPeriodId, attendanceWeekId, model,
            CancellationToken);

        return NoContent();
    }

    [HttpPut("reg-group/{regGroupId:guid}/{attendancePeriodId:guid}/{attendanceWeekId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarks)]
    public async Task<IActionResult> SubmitRegGroupRegisterAsync([FromRoute] Guid regGroupId,
        [FromRoute] Guid attendancePeriodId, [FromRoute] Guid attendanceWeekId,
        [FromBody] SubmitRegisterRequest model)
    {
        await _registerService.SubmitRegGroupRegisterAsync(regGroupId, attendancePeriodId, attendanceWeekId,
            model, CancellationToken);

        return NoContent();
    }
}
