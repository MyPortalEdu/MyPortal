namespace MyPortal.Contracts.Models.People;

/// <summary>One period of the staff member's reporting line. A null EndDate is the current manager.</summary>
public class StaffLineManagerHistoryResponse
{
    public Guid Id { get; set; }
    public Guid LineManagerId { get; set; }
    public string? LineManagerName { get; set; }
    public string? LineManagerCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
