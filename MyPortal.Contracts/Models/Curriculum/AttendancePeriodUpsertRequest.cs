namespace MyPortal.Contracts.Models.Curriculum;

public class AttendancePeriodUpsertRequest
{
    public Guid? AttendancePeriodId { get; set; }
    
    public int CycleDayIndex { get; set; }
    
    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAmReg { get; set; }

    public bool IsPmReg { get; set; }

    public bool IsLesson { get; set; }
}