namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceMarksRequest
{
    public Guid StudentGroupId { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public IList<BulkAttendanceMarkUpsert> Marks { get; set; } = new List<BulkAttendanceMarkUpsert>();
}
