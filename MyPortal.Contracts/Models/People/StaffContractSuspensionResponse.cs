namespace MyPortal.Contracts.Models.People;

public class StaffContractSuspensionResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
