namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// Write payload for attaching a contact role to an existing <c>Person</c>. The person's bio is
/// already on file (created with another subtype role — e.g. as a student, or a former contact), so
/// no bio fields are supplied here, which would risk silently overwriting the shared Person record.
/// Used by the "new contact" flow when the office picks an existing person rather than creating one.
/// </summary>
public class ContactCreateForPersonRequest
{
    public Guid PersonId { get; set; }
}
