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

/// <summary>Bulk attendance-mark endpoints.</summary>
public sealed class AttendanceMarksController(
    ProblemDetailsFactory problemFactory,
    ILogger<AttendanceMarksController> logger,
    IAttendanceMarkService attendanceMarkService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get attendance marks for a group across a date range.</summary>
    /// <remarks>Returns the full student-by-period matrix, including blank cells.</remarks>
    /// <param name="studentGroupId">The student group whose marks to fetch.</param>
    /// <param name="from">Start of the range (inclusive, local date).</param>
    /// <param name="to">End of the range (inclusive, local date).</param>
    [HttpGet("bulk")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(BulkAttendanceMarksResponse), 200)]
    public async Task<IActionResult> GetBulkAsync([FromQuery] Guid studentGroupId,
        [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await attendanceMarkService.GetBulkAsync(studentGroupId, from, to, CancellationToken);

        return Ok(result);
    }

    /// <summary>Apply a batch of attendance-mark edits.</summary>
    /// <remarks>A null <c>AttendanceCodeId</c> clears the mark for that cell.</remarks>
    [HttpPut("bulk")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.EditAttendanceMarksBulk)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SubmitBulkAsync([FromBody] BulkAttendanceMarksRequest model)
    {
        await attendanceMarkService.SubmitBulkAsync(model, CancellationToken);

        return NoContent();
    }
}
