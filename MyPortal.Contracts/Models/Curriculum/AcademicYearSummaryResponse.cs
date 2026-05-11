namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicYearSummaryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsLocked { get; set; }

    public int TimetableCycleLength { get; set; }

    public int SchoolWeekLength { get; set; }

    // Derived from MIN(term.StartDate) / MAX(term.EndDate); null if the AY has no
    // terms (shouldn't happen in normal data — at-least-one is enforced on create).
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
