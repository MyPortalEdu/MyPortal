namespace MyPortal.Contracts.Models.Pastoral;

public class HouseSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? ColourCode { get; set; }
    public string? MainSupervisorName { get; set; }
}
