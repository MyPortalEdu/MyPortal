namespace MyPortal.Contracts.Models.Pastoral;

public class RegGroupSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string YearGroupName { get; set; } = null!;
    public string? MainSupervisorName { get; set; }
}