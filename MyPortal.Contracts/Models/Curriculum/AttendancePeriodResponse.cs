namespace MyPortal.Contracts.Models.Curriculum;

public class AttendancePeriodResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int CycleDayIndex { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAmReg { get; set; }

    public bool IsPmReg { get; set; }

    public bool IsLesson { get; set; }
}
