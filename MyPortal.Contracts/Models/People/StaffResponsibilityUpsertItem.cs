namespace MyPortal.Contracts.Models.People;

/// <summary>A responsibility row in the upsert. Null id is a new assignment; a populated id
/// updates; omitted rows are soft-deleted server-side. A null EndDate means still current.</summary>
public class StaffResponsibilityUpsertItem
{
    public Guid? Id { get; set; }
    public Guid ResponsibilityTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
