using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Services.Interfaces.Attendance;

public interface IAttendanceCodeService
{
    Task<IList<AttendanceCodeResponse>> GetActiveAsync(CancellationToken cancellationToken);

    // Cross-references the codes referenced by a set of marks against the database:
    // throws if any code is unknown, inactive, or restricted (the latter unless the
    // caller has Attendance.UseRestrictedCodes). Empty input is a no-op.
    Task EnsureCodesAreUsableAsync(IEnumerable<Guid> codeIds, CancellationToken cancellationToken);
}
