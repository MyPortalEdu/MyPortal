using MyPortal.Contracts.Models.Attendance;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IAttendanceMarkRepository
{
    // Returns null when the StudentGroup doesn't exist (caller maps to 404).
    Task<BulkAttendanceMarksResponse?> GetBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        CancellationToken cancellationToken);

    Task SubmitBulkAsync(Guid studentGroupId, DateTime from, DateTime to,
        IReadOnlyCollection<BulkAttendanceMarkUpsert> marks, CancellationToken cancellationToken);
}
