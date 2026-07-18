using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Attendance;

namespace MyPortal.Services.Attendance;

public class AttendanceCodeService(
    IAuthorizationService authorizationService,
    ILogger<AttendanceCodeService> logger,
    IAttendanceCodeRepository attendanceCodeRepository)
    : BaseService(authorizationService, logger), IAttendanceCodeService
{
    public async Task<IList<AttendanceCodeResponse>> GetActiveAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Attendance.ViewAttendanceMarks,
            cancellationToken);

        var codes = await attendanceCodeRepository.GetActiveAsync(cancellationToken);

        return codes.Select(c => new AttendanceCodeResponse
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            AttendanceCodeTypeId = c.AttendanceCodeTypeId,
            IsRestricted = c.IsRestricted
        }).ToList();
    }

    public async Task EnsureCodesAreUsableAsync(IEnumerable<Guid> codeIds, CancellationToken cancellationToken)
    {
        var distinct = codeIds.Distinct().ToList();
        if (distinct.Count == 0) return;

        var codes = await attendanceCodeRepository.GetByIdsAsync(distinct, cancellationToken);
        var codesById = codes.ToDictionary(c => c.Id);

        var unknown = distinct.Where(id => !codesById.ContainsKey(id)).ToList();
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
