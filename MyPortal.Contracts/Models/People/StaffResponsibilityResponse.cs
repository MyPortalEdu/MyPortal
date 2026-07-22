namespace MyPortal.Contracts.Models.People;

/// <summary>A single designated-responsibility assignment (e.g. DSL, First Aider) for a staff member.</summary>
public class StaffResponsibilityResponse
{
    public Guid Id { get; set; }
    public Guid ResponsibilityTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
