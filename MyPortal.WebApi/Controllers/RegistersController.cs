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

/// <summary>
/// The teacher-facing register flow — fetch one register's worth of students for a
/// specific period+week, then submit marks for it. For the wider "edit any cell"
/// admin grid, see <see cref="AttendanceMarksController"/>.
/// </summary>
public sealed class RegistersController : BaseApiController<RegistersController>
{
    private readonly IRegisterService _registerService;

    public RegistersController(ProblemDetailsFactory problemFactory, ILogger<RegistersController> logger,
        IRegisterService registerService) : base(problemFactory, logger)
    {
        _registerService = registerService;
    }

    /// <summary>Get the register for a lesson session in a given attendance week.</summary>
    /// <remarks>
    /// A "lesson" register is taken against a <c>SessionPeriod</c> (a teaching
    /// session in the timetable). Returns the student roster, any existing marks,
    /// and the cell shape needed to render the register grid.
    /// </remarks>
    /// <param name="sessionPeriodId">The session period whose register to load.</param>
    /// <param name="attendanceWeekId">The attendance week (calendar week within the AY) to load marks for.</param>
    [HttpGet("lesson/{sessionPeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    public async Task<IActionResult> GetLessonRegisterAsync([FromRoute] Guid sessionPeriodId,
        [FromRoute] Guid attendanceWeekId)
    {
        var result = await _registerService.GetLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the AM/PM statutory register for a registration group.</summary>
    /// <remarks>
    /// Reg-group registers are the statutory AM/PM marks for a tutor group. The
    /// <paramref name="attendancePeriodId"/> identifies which sitting (AM or PM)
    /// within the cycle day.
    /// </remarks>
    /// <param name="regGroupId">The registration (tutor) group.</param>
    /// <param name="attendancePeriodId">The attendance period (AM or PM sitting).</param>
    /// <param name="attendanceWeekId">The attendance week to load marks for.</param>
    [HttpGet("reg-group/{regGroupId:guid}/{attendancePeriodId:guid}/{attendanceWeekId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    public async Task<IActionResult> GetRegGroupRegisterAsync([FromRoute] Guid regGroupId,
        [FromRoute] Guid attendancePeriodId, [FromRoute] Guid attendanceWeekId)
    {
        var result = await _registerService.GetRegGroupRegisterAsync(regGroupId, attendancePeriodId,
            attendanceWeekId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Submit a teacher's marks for a lesson register.</summary>
    /// <remarks>
    /// Upserts marks for every student in the register. Restricted codes (e.g.
    /// <c>B</c>, <c>K</c>, <c>E</c>) are silently ignored unless the caller has
    /// <c>Attendance.UseRestrictedCodes</c>.
    /// </remarks>
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
        await _registerService.SubmitLessonRegisterAsync(sessionPeriodId, attendanceWeekId, model,
            CancellationToken);

        return NoContent();
    }

    /// <summary>Submit a tutor's AM/PM marks for a registration group.</summary>
    /// <remarks>
    /// Same submission semantics as the lesson variant; the period+week+group
    /// combination identifies the register. Restricted codes are filtered as on
    /// the lesson endpoint.
    /// </remarks>
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
        await _registerService.SubmitRegGroupRegisterAsync(regGroupId, attendancePeriodId, attendanceWeekId,
            model, CancellationToken);

        return NoContent();
    }
}
