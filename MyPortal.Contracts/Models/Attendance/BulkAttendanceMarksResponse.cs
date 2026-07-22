namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceMarksResponse
{
    public Guid StudentGroupId { get; set; }

    public string GroupCode { get; set; } = null!;

    public DateTime From { get; set; }

    public DateTime To { get; set; }
    
    public IList<BulkAttendancePeriodInstanceResponse> Periods { get; set; }
        = new List<BulkAttendancePeriodInstanceResponse>();
    
    public IList<BulkAttendanceStudentResponse> Students { get; set; }
        = new List<BulkAttendanceStudentResponse>();

    public IList<RegisterMarkResponse> Marks { get; set; } = new List<RegisterMarkResponse>();
}
