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
/// Bulk read/write of attendance marks across a (student group, date range) grid —
/// the reception/admin "edit any cell" view. For single-period registers see
/// <see cref="RegistersController"/>.
/// </summary>
public sealed class AttendanceMarksController : BaseApiController<AttendanceMarksController>
{
    private readonly IAttendanceMarkService _attendanceMarkService;

    public AttendanceMarksController(ProblemDetailsFactory problemFactory,
        ILogger<AttendanceMarksController> logger, IAttendanceMarkService attendanceMarkService)
        : base(problemFactory, logger)
    {
        _attendanceMarkService = attendanceMarkService;
    }

    /// <summary>Get the attendance grid for a student group across a date range.</summary>
    /// <remarks>
    /// Returns the full (student × period × week) matrix for the group between
    /// <paramref name="from"/> and <paramref name="to"/>, including any blank cells.
    /// Range is bounded server-side; very wide ranges may be rejected to protect
    /// the database.
    /// </remarks>
    /// <param name="studentGroupId">The student group whose marks to fetch.</param>
    /// <param name="from">Start of the range (inclusive, local date).</param>
    /// <param name="to">End of the range (inclusive, local date).</param>
    [HttpGet("bulk")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(BulkAttendanceMarksResponse), 200)]
    public async Task<IActionResult> GetBulkAsync([FromQuery] Guid studentGroupId,
        [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _attendanceMarkService.GetBulkAsync(studentGroupId, from, to, CancellationToken);

        return Ok(result);
    }

    /// <summary>Apply a batch of attendance mark edits in one go.</summary>
    /// <remarks>
    /// Each entry is keyed by (StudentId, AttendanceWeekId, AttendancePeriodId). A
    /// null <c>AttendanceCodeId</c> on an entry signals "delete the existing mark
    /// for that cell" so corrections can clear an entry rather than overwrite it.
    /// Requires <c>Attendance.EditAttendanceMarksBulk</c> — narrower than the regular
    /// register edit permission since this lets the caller mutate any cell.
    /// </remarks>
    [HttpPut("bulk")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarksBulk)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SubmitBulkAsync([FromBody] BulkAttendanceMarksRequest model)
    {
        await _attendanceMarkService.SubmitBulkAsync(model, CancellationToken);

        return NoContent();
    }
}
