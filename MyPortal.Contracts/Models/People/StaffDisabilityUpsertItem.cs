namespace MyPortal.Contracts.Models.People;

public class StaffDisabilityUpsertItem
{
    public Guid DisabilityId { get; set; }
    public DateTime? DateAdvised { get; set; }
    public bool IsLongTerm { get; set; }
    public bool AffectsWorkingAbility { get; set; }
    public string? AssistanceRequired { get; set; }
}
