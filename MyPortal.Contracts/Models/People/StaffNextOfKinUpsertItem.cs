namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A next-of-kin row in the upsert. A null <see cref="Id"/> is a new link; a populated id updates;
/// omitted rows are soft-deleted (the link only — the shared Contact is never deleted here).
///
/// Set <see cref="PersonId"/> to reuse an existing person as the contact (search-before-add);
/// leave it null to create a fresh contact from the name fields.
/// </summary>
public class StaffNextOfKinUpsertItem
{
    public Guid? Id { get; set; }

    public Guid? PersonId { get; set; }

    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? Gender { get; set; }

    public Guid? RelationshipTypeId { get; set; }
    public int ContactOrder { get; set; }
    public string? Notes { get; set; }

    public List<PersonPhoneUpsertItem> Phones { get; set; } = [];
    public List<PersonEmailUpsertItem> Emails { get; set; } = [];
}
