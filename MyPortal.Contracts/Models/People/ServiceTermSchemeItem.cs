namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pension scheme offered by a service term. At most one entry per term may be flagged
/// <see cref="IsMain"/>; that is the scheme a new contract on the term defaults to.
/// </summary>
public class ServiceTermSchemeItem
{
    public Guid SuperannuationSchemeId { get; set; }
    public bool IsMain { get; set; }
}
