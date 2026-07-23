namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pay scale option. Carries its service term so the contract editor can offer only the scales
/// that belong to the term the contract is held on.
/// </summary>
public class PayScaleResponse
{
    public Guid Id { get; set; }
    public Guid ServiceTermId { get; set; }
    public string Description { get; set; } = null!;
}
