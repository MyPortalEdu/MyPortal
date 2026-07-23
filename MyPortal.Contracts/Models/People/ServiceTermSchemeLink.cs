namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Which pension schemes a service term offers, and which one new contracts default to. A scheme
/// can appear on several terms, so this is a link list rather than a field on the scheme.
/// </summary>
public class ServiceTermSchemeLink
{
    public Guid ServiceTermId { get; set; }
    public Guid SuperannuationSchemeId { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>
/// The default hours and weeks a service term contributes to a new contract. Pre-filled when the
/// term is chosen and overtypeable, so a part-time or term-time contract can differ.
/// </summary>
public class ServiceTermDefaultsItem
{
    public Guid ServiceTermId { get; set; }
    public decimal? HoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }
}
