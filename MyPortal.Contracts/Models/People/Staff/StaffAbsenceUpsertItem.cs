namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>An absence row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are deleted server-side (scoped so a line manager never affects
/// confidential rows). IsConfidential is honoured only for All-scope (HR) editors.</summary>
public class StaffAbsenceUpsertItem
{
    public Guid? Id { get; set; }
    public Guid AbsenceTypeId { get; set; }
    public Guid? IllnessTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsConfidential { get; set; }
    public string? Notes { get; set; }
}
