namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>One row of the Staff Training report: a training record a staff member holds.</summary>
public class StaffTrainingReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string? Course { get; set; }
    public string? Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? Hours { get; set; }
    public string? Provider { get; set; }
}

/// <summary>One attendee for the Training Course report (staff holding a record for a given course).</summary>
public class TrainingCourseAttendeeReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string? Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? Hours { get; set; }
}

/// <summary>A selectable training course for the Training Course report's course picker.</summary>
public class TrainingCourseOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
