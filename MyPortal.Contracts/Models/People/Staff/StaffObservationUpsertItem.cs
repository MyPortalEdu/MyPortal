namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>An observation row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are removed server-side.</summary>
public class StaffObservationUpsertItem
{
    public Guid? Id { get; set; }
    public DateTime Date { get; set; }
    public Guid ObserverId { get; set; }
    public Guid OutcomeId { get; set; }
    public string? Focus { get; set; }
    public string? SubjectObserved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForDevelopment { get; set; }
    public string? Notes { get; set; }
}
