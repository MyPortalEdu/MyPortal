namespace MyPortal.Contracts.Models.People;

public class SuperannuationSchemeResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = null!;

    public decimal? EmployerRate { get; set; }
}
