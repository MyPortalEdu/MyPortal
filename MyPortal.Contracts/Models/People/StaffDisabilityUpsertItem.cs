namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One declared disability in the equality payload. Matched on <see cref="DisabilityId"/> — a
/// staff member can't declare the same disability twice; rows absent from the payload are removed.
/// </summary>
public class StaffDisabilityUpsertItem
{
    public Guid DisabilityId { get; set; }
    public DateTime? DateAdvised { get; set; }
    public bool IsLongTerm { get; set; }
    public bool AffectsWorkingAbility { get; set; }
    public string? AssistanceRequired { get; set; }
}
