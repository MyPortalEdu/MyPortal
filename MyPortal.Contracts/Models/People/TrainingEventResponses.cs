namespace MyPortal.Contracts.Models.People;

public class TrainingEventSummaryResponse
{
    public Guid Id { get; set; }
    public string CourseName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Location { get; set; }
    public string? Trainer { get; set; }
    public int AttendeeCount { get; set; }
    public int? Capacity { get; set; }
}

public class TrainingEventAttendeeResponse
{
    public Guid StaffMemberId { get; set; }
    public string StaffName { get; set; } = null!;
    public string StaffCode { get; set; } = null!;
    public bool? HasAttended { get; set; }
}

public class TrainingEventDetailsResponse
{
    public Guid Id { get; set; }
    public Guid TrainingCourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Location { get; set; }
    public string? Trainer { get; set; }
    public string? Provider { get; set; }
    public decimal? Hours { get; set; }
    public int? Capacity { get; set; }
    public string? Notes { get; set; }
    public List<TrainingEventAttendeeResponse> Attendees { get; set; } = [];
}
