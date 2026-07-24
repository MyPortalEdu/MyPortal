namespace MyPortal.Contracts.Models.People;

public class StaffLineManagerHistoryResponse
{
    public Guid Id { get; set; }
    public Guid LineManagerId { get; set; }
    public string? LineManagerName { get; set; }
    public string? LineManagerCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
