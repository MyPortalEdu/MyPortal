namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// A candidate existing <c>Person</c> surfaced by the "new contact" search. Because a Person can
/// hold several subtype roles (staff/student/contact/agent), the office searches existing People
/// first so someone already on file (a pupil, a former contact, ...) gets a contact role attached to
/// their existing Person rather than a duplicate Person row.
///
/// <see cref="ExistingContactId"/> is set when the person already has a (non-deleted) contact
/// record — the UI blocks creating a duplicate contact and deep-links to the existing profile.
/// </summary>
public class ContactMatchResponse
{
    public Guid PersonId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public DateTime? Dob { get; set; }

    /// <summary>The person's existing Contact id, or null if they aren't (active) a contact.</summary>
    public Guid? ExistingContactId { get; set; }

    /// <summary>True when the person already has an active contact record. Derived, read-only.</summary>
    public bool IsContact => ExistingContactId.HasValue;
}
