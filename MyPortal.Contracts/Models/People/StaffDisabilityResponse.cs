namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A declared disability with the detail the Equality Act duty needs — when it was advised,
/// whether it is long-term, whether it affects working ability, and the adjustments agreed.
/// </summary>
public class StaffDisabilityResponse
{
    public Guid DisabilityId { get; set; }
    public DateTime? DateAdvised { get; set; }
    public bool IsLongTerm { get; set; }
    public bool AffectsWorkingAbility { get; set; }
    public string? AssistanceRequired { get; set; }
}
