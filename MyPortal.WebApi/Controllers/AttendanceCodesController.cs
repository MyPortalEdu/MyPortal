using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Read-only access to the school's active attendance codes (DfE 2024 codes plus
/// any school-defined additions). Used by the register UI to populate the code
/// dropdowns. Code authoring/restriction is managed via attendance settings, not
/// this controller.
/// </summary>
public sealed class AttendanceCodesController : BaseApiController<AttendanceCodesController>
{
    private readonly IAttendanceCodeService _attendanceCodeService;

    public AttendanceCodesController(ProblemDetailsFactory problemFactory,
        ILogger<AttendanceCodesController> logger, IAttendanceCodeService attendanceCodeService)
        : base(problemFactory, logger)
    {
        _attendanceCodeService = attendanceCodeService;
    }

    /// <summary>List every active attendance code, including restricted ones.</summary>
    /// <remarks>
    /// All active codes are returned regardless of restriction; the UI greys out
    /// restricted entries for users without <c>Attendance.UseRestrictedCodes</c>.
    /// Inactive codes (e.g. legacy <c>H</c>, <c>J</c>, generic <c>Y</c>) are excluded.
    /// </remarks>
    [HttpGet("active")]
    [Permission(PermissionMode.RequireAny, Permissions.Attendance.ViewAttendanceMarks)]
    [ProducesResponseType(typeof(IList<AttendanceCodeResponse>), 200)]
    public async Task<IActionResult> GetActiveAsync()
    {
        var result = await _attendanceCodeService.GetActiveAsync(CancellationToken);

        return Ok(result);
    }
}
