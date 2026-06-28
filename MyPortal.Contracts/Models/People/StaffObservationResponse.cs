namespace MyPortal.Contracts.Models.People;

/// <summary>A lesson observation where the staff member was the observee.</summary>
public class StaffObservationResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public Guid ObserverId { get; set; }
    public Guid OutcomeId { get; set; }
    public string? Focus { get; set; }
    public string? SubjectObserved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForDevelopment { get; set; }
    public string? Notes { get; set; }
}
