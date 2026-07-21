namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Contact Details area. Each list is a whole-collection replace: the
/// server diffs against what's stored — inserting new rows, updating matched ids, and
/// soft-deleting any row no longer present.
/// </summary>
public class PersonContactDetailsUpsertRequest
{
    public List<PersonEmailUpsertItem> Emails { get; set; } = [];
    public List<PersonPhoneUpsertItem> Phones { get; set; } = [];
}
