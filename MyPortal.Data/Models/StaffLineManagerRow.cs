namespace MyPortal.Data.Models;

public class StaffLineManagerRow
{
    public Guid Id { get; set; }
    public Guid LineManagerId { get; set; }
    public string? LineManagerName { get; set; }
    public string? LineManagerCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
