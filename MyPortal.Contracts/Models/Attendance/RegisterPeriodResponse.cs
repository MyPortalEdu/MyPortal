namespace MyPortal.Contracts.Models.Attendance;

public class RegisterPeriodResponse
{
    public Guid AttendancePeriodId { get; set; }

    public string PeriodName { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAmReg { get; set; }

    public bool IsPmReg { get; set; }
}
