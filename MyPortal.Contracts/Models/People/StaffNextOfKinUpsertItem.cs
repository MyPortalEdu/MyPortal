namespace MyPortal.Contracts.Models.People;

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
