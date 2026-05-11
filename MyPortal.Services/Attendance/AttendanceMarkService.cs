using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.Services.Attendance;

public class AttendanceMarkService : BaseService, IAttendanceMarkService
{
    private readonly IAttendanceMarkRepository _attendanceMarkRepository;
    private readonly IAttendanceCodeService _attendanceCodeService;
    private readonly IValidationService _validationService;

    public AttendanceMarkService(IAuthorizationService authorizationService, ILogger<AttendanceMarkService> logger,
        IAttendanceMarkRepository attendanceMarkRepository, IAttendanceCodeService attendanceCodeService,
        IValidationService validationService)
        : base(authorizationService, logger)
    {
        _attendanceMarkRepository = attendanceMarkRepository;
        _attendanceCodeService = attendanceCodeService;
        _validationService = validationService;
    }

    public async Task<BulkAttendanceMarksResponse> GetBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        if (from > to)
        {
            throw new ArgumentException("From date must be on or before To date.");
        }

        var result = await _attendanceMarkRepository.GetBulkAsync(studentGroupId, from, to, cancellationToken);

        return result ?? throw new NotFoundException("Student group not found.");
    }

    public async Task SubmitBulkAsync(BulkAttendanceMarksRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.EditAttendanceMarksBulk,
            cancellationToken);

        await _validationService.ValidateAsync(model);

        // Null AttendanceCodeId entries are deletes — skip them in the policy check.
        await _attendanceCodeService.EnsureCodesAreUsableAsync(
            model.Marks.Where(m => m.AttendanceCodeId.HasValue)
                       .Select(m => m.AttendanceCodeId!.Value),
            cancellationToken);

        await _attendanceMarkRepository.SubmitBulkAsync(model.StudentGroupId, model.From, model.To,
            model.Marks.ToList(), cancellationToken);

        Logger.LogInformation(
            "Bulk attendance marks submitted: group {studentGroupId}, [{from} .. {to}], {markCount} cells",
            model.StudentGroupId, model.From, model.To, model.Marks.Count);
    }
}
