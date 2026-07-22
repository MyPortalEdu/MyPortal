namespace MyPortal.Contracts.Models.People;

/// <summary>A person who contributes to a Personal Education Plan, with their display name.</summary>
public class PepContributorResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string PersonName { get; set; } = null!;
}
