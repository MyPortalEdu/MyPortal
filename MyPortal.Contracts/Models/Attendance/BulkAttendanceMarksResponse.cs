namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceMarksResponse
{
    public Guid StudentGroupId { get; set; }

    public string GroupCode { get; set; } = null!;

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    // Every (week, period) instance whose actual date falls within [From, To]
    // for the StudentGroup's academic year. Forms the column axis of the grid.
    public IList<BulkAttendancePeriodInstanceResponse> Periods { get; set; }
        = new List<BulkAttendancePeriodInstanceResponse>();

    // Regular roster (date-effective members of the group) plus any extras
    // attached via SessionExtraNames to a Session of this group's curriculum
    // within range. IsExtra distinguishes the two.
    public IList<BulkAttendanceStudentResponse> Students { get; set; }
        = new List<BulkAttendanceStudentResponse>();

    public IList<RegisterMarkResponse> Marks { get; set; } = new List<RegisterMarkResponse>();
}
