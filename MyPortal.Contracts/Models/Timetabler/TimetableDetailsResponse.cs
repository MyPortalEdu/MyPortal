namespace MyPortal.Contracts.Models.Timetabler;

public class TimetableDetailsResponse
{
    public Guid Id { get; set; }
    public Guid AcademicYearId { get; set; }
    public string Name { get; set; } = null!;
    public int Status { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public long Version { get; set; }
}
