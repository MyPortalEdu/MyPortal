namespace MyPortal.Contracts.Models.Attendance;

public class RegisterMarkResponse
{
    public Guid AttendanceMarkId { get; set; }

    public Guid StudentId { get; set; }
    
    public Guid AttendancePeriodId { get; set; }

    public Guid AttendanceCodeId { get; set; }

    public string Code { get; set; } = null!;

    public string? Comments { get; set; }

    public int? MinutesLate { get; set; }
}
