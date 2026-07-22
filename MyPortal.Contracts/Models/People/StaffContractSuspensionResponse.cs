namespace MyPortal.Contracts.Models.People;

/// <summary>A period this contract was suspended. A null <see cref="EndDate"/> is still in force.</summary>
public class StaffContractSuspensionResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
