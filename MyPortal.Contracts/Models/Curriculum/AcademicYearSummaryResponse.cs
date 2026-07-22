namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicYearSummaryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsLocked { get; set; }

    public int TimetableCycleLength { get; set; }

    public int SchoolWeekLength { get; set; }
    
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
