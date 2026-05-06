using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Services.Interfaces.Attendance;

public interface IAttendanceMarkService
{
    Task<BulkAttendanceMarksResponse> GetBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        CancellationToken cancellationToken);

    Task SubmitBulkAsync(BulkAttendanceMarksRequest model, CancellationToken cancellationToken);
}
