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

/// <summary>Attendance register endpoints.</summary>
public sealed class RegistersController(
    ProblemDetailsFactory problemFactory,
    ILogger<RegistersController> logger,
    IRegisterService registerService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get the register for a lesson session.</summary>
    /// <remarks>Returns the roster, existing marks, and cell shape for the register grid.</remarks>
    /// <param name="sessionPeriodId">The session period whose register to load.</param>
    /// <param name="attendanceWeekId">The attendance week (calendar week within the AY) to load marks for.</param>
    [HttpGet("lesson/{sessionPeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    public async Task<IActionResult> GetLessonRegisterAsync([FromRoute] Guid sessionPeriodId,
        [FromRoute] Guid attendanceWeekId)
    {
        var result = await registerService.GetLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the AM/PM statutory register for a registration group.</summary>
    /// <remarks><paramref name="attendancePeriodId"/> selects the AM or PM sitting.</remarks>
    /// <param name="regGroupId">The registration (tutor) group.</param>
    /// <param name="attendancePeriodId">The attendance period (AM or PM sitting).</param>
    /// <param name="attendanceWeekId">The attendance week to load marks for.</param>
    [HttpGet("reg-group/{regGroupId:guid}/{attendancePeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    public async Task<IActionResult> GetRegGroupRegisterAsync([FromRoute] Guid regGroupId,
        [FromRoute] Guid attendancePeriodId, [FromRoute] Guid attendanceWeekId)
    {
        var result = await registerService.GetRegGroupRegisterAsync(regGroupId, attendancePeriodId,
            attendanceWeekId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Submit a teacher's marks for a lesson register.</summary>
    /// <remarks>Restricted codes are ignored unless the caller can use them.</remarks>
    /// <param name="sessionPeriodId">The session period the register belongs to.</param>
    /// <param name="attendanceWeekId">The attendance week the register is for.</param>
    /// <param name="model">The student-keyed marks to submit.</param>
    [HttpPut("lesson/{sessionPeriodId:guid}/{attendanceWeekId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarks)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SubmitLessonRegisterAsync([FromRoute] Guid sessionPeriodId,
        [FromRoute] Guid attendanceWeekId, [FromBody] SubmitRegisterRequest model)
    {
        await registerService.SubmitLessonRegisterAsync(sessionPeriodId, attendanceWeekId, model,
            CancellationToken);

        return NoContent();
    }

    /// <summary>Submit a tutor's AM/PM marks for a registration group.</summary>
    /// <remarks>Same submission semantics as the lesson variant.</remarks>
    /// <param name="regGroupId">The registration (tutor) group.</param>
    /// <param name="attendancePeriodId">The AM or PM sitting.</param>
    /// <param name="attendanceWeekId">The attendance week.</param>
    /// <param name="model">The student-keyed marks to submit.</param>
    [HttpPut("reg-group/{regGroupId:guid}/{attendancePeriodId:guid}/{attendanceWeekId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarks)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SubmitRegGroupRegisterAsync([FromRoute] Guid regGroupId,
        [FromRoute] Guid attendancePeriodId, [FromRoute] Guid attendanceWeekId,
        [FromBody] SubmitRegisterRequest model)
    {
        await registerService.SubmitRegGroupRegisterAsync(regGroupId, attendancePeriodId, attendanceWeekId,
            model, CancellationToken);

        return NoContent();
    }
}
