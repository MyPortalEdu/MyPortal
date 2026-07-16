namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One phone number in a contact-details replace payload. A null <see cref="Id"/> is a new
/// number; a populated <see cref="Id"/> updates the existing row. Rows present in the database
/// but absent from the payload are soft-deleted.
/// </summary>
public class PersonPhoneUpsertItem
{
    public Guid? Id { get; set; }
    public Guid TypeId { get; set; }
    public string Number { get; set; } = null!;
    public bool IsMain { get; set; }
}
