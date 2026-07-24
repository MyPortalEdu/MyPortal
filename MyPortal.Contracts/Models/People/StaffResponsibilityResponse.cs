namespace MyPortal.Contracts.Models.People;

public class StaffResponsibilityResponse
{
    public Guid Id { get; set; }
    public Guid ResponsibilityTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
