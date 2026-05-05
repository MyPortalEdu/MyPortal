using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.Services.Attendance;

public class RegisterService : BaseService, IRegisterService
{
    private readonly IRegisterRepository _registerRepository;
    private readonly IAttendanceCodeRepository _attendanceCodeRepository;
    private readonly IValidationService _validationService;

    public RegisterService(IAuthorizationService authorizationService, ILogger<RegisterService> logger,
        IRegisterRepository registerRepository, IAttendanceCodeRepository attendanceCodeRepository,
        IValidationService validationService)
        : base(authorizationService, logger)
    {
        _registerRepository = registerRepository;
        _attendanceCodeRepository = attendanceCodeRepository;
        _validationService = validationService;
    }

    public async Task<RegisterResponse> GetLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        var register = await _registerRepository.GetLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
            cancellationToken);

        return register ?? throw new NotFoundException("Register not found.");
    }

    public async Task<RegisterResponse> GetRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId,
        Guid attendanceWeekId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        var register = await _registerRepository.GetRegGroupRegisterAsync(regGroupId, attendancePeriodId,
            attendanceWeekId, cancellationToken);

        return register ?? throw new NotFoundException("Register not found.");
    }

    public async Task SubmitLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        SubmitRegisterRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.EditAttendanceMarks,
            cancellationToken);

        await _validationService.ValidateAsync(model);

        await EnsureCodesAreUsableAsync(model.Marks, cancellationToken);

        await _registerRepository.SubmitLessonRegisterAsync(sessionPeriodId, attendanceWeekId,
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

        await _validationService.ValidateAsync(model);

        await EnsureCodesAreUsableAsync(model.Marks, cancellationToken);

        await _registerRepository.SubmitRegGroupRegisterAsync(regGroupId, attendancePeriodId, attendanceWeekId,
            model.Marks.ToList(), cancellationToken);

        Logger.LogInformation(
            "Reg-group register submitted: regGroup {regGroupId}, period {attendancePeriodId}, week {attendanceWeekId}, {markCount} marks",
            regGroupId, attendancePeriodId, attendanceWeekId, model.Marks.Count);
    }

    /// <summary>
    /// Cross-references the codes referenced by the submitted marks against the database:
    /// rejects unknown codes, inactive codes, and restricted codes when the caller doesn't
    /// hold UseRestrictedCodes. The basic validator ensures non-empty Guid and shape; this
    /// is the data-driven half that the validator can't run synchronously.
    /// </summary>
    private async Task EnsureCodesAreUsableAsync(IEnumerable<SubmitMarkRequest> marks,
        CancellationToken cancellationToken)
    {
        var codeIds = marks.Select(m => m.AttendanceCodeId).Distinct().ToList();
        if (codeIds.Count == 0) return;

        var codes = await _attendanceCodeRepository.GetByIdsAsync(codeIds, cancellationToken);
        var codesById = codes.ToDictionary(c => c.Id);

        var unknown = codeIds.Where(id => !codesById.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
        {
            throw new NotFoundException(
                $"Unknown attendance code(s): {string.Join(", ", unknown)}");
        }

        var inactive = codes.Where(c => !c.IsActive).Select(c => c.Code).ToList();
        if (inactive.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot submit inactive attendance code(s): {string.Join(", ", inactive)}");
        }

        if (codes.Any(c => c.IsRestricted))
        {
            var canUseRestricted = await AuthorizationService.HasPermissionAsync(
                Permissions.Attendance.UseRestrictedCodes, cancellationToken);

            if (!canUseRestricted)
            {
                var restricted = codes.Where(c => c.IsRestricted).Select(c => c.Code).ToList();
                throw new ForbiddenException(
                    $"You do not have permission to use restricted attendance code(s): {string.Join(", ", restricted)}");
            }
        }
    }
}
