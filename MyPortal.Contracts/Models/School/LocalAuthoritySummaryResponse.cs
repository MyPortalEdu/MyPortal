namespace MyPortal.Contracts.Models.School;

public class LocalAuthoritySummaryResponse
{
    public Guid Id { get; set; }
    public int LeaCode { get; set; }
    public string Name { get; set; } = null!;
}
