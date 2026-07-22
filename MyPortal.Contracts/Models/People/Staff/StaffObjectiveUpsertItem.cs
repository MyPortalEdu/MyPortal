namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>An objective row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are soft-deleted server-side.</summary>
public class StaffObjectiveUpsertItem
{
    public Guid? Id { get; set; }
    public Guid? ReviewId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? SuccessCriteria { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? StatusId { get; set; }
    public string? ProgressNotes { get; set; }
}
