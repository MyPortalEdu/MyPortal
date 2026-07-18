using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.Services.Attendance;

public class RegisterService(
    IAuthorizationService authorizationService,
    ILogger<RegisterService> logger,
    IRegisterRepository registerRepository,
    IAttendanceCodeService attendanceCodeService,
    IValidationService validationService)
    : BaseService(authorizationService, logger), IRegisterService
{
    public async Task<RegisterResponse> GetLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        var register = await registerRepository.GetLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            cancellationToken);

        return register ?? throw new NotFoundException("Register not found.");
    }

    public async Task<RegisterResponse> GetRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId,
        Guid attendanceWeekId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        var register = await registerRepository.GetRegGroupRegisterAsync(regGroupId, attendancePeriodId,
            attendanceWeekId, cancellationToken);

        return register ?? throw new NotFoundException("Register not found.");
    }

    public async Task SubmitLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        SubmitRegisterRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.EditAttendanceMarks,
            cancellationToken);

        await validationService.ValidateAsync(model);

        await attendanceCodeService.EnsureCodesAreUsableAsync(
            model.Marks.Select(m => m.AttendanceCodeId), cancellationToken);

        await registerRepository.SubmitLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            model.Marks.ToList(), cancellationToken);

        Logger.LogInformation(
            "Lesson register submitted: sessionPeriod {sessionPeriodId}, week {attendanceWeekId}, {markCount} marks",
            sessionPeriodId, attendanceWeekId, model.Marks.Count);
    }

    public async Task SubmitRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId, Guid attendanceWeekId,
        SubmitRegisterRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.EditAttendanceMarks,
            cancellationToken);

        await validationService.ValidateAsync(model);

        await attendanceCodeService.EnsureCodesAreUsableAsync(
            model.Marks.Select(m => m.AttendanceCodeId), cancellationToken);

        await registerRepository.SubmitRegGroupRegisterAsync(regGroupId, attendancePeriodId, attendanceWeekId,
            model.Marks.ToList(), cancellationToken);

        Logger.LogInformation(
            "Reg-group register submitted: regGroup {regGroupId}, period {attendancePeriodId}, week {attendanceWeekId}, {markCount} marks",
            regGroupId, attendancePeriodId, attendanceWeekId, model.Marks.Count);
    }
}
