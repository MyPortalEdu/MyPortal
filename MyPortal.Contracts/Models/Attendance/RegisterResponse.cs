namespace MyPortal.Contracts.Models.Attendance;

public class RegisterResponse
{
    public Guid AttendanceWeekId { get; set; }

    public Guid AttendancePeriodId { get; set; }

    public string PeriodName { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAmReg { get; set; }

    public bool IsPmReg { get; set; }

    public string GroupCode { get; set; } = null!;

    public Guid? SessionPeriodId { get; set; }

    public Guid? RegGroupId { get; set; }

    public Guid? TeacherId { get; set; }

    public string? TeacherName { get; set; }

    public Guid? RoomId { get; set; }

    public string? RoomName { get; set; }

    public bool IsCover { get; set; }
    
    public IList<RegisterPeriodResponse> Periods { get; set; } = new List<RegisterPeriodResponse>();

    public IList<RegisterStudentResponse> Students { get; set; } = new List<RegisterStudentResponse>();

    public IList<RegisterMarkResponse> Marks { get; set; } = new List<RegisterMarkResponse>();
}
