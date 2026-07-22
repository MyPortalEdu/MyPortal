namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// Lightweight contact row for "pick a contact" UI and the contact list. <see cref="Id"/> is the
/// Contact id (the key for the contact profile / CRUD endpoints); <see cref="PersonId"/> is the
/// underlying Person id, the value to write into person-FK columns. <see cref="LinkedStudentCount"/>
/// is how many students this contact is linked to (relationship rows).
/// </summary>
public class ContactSummaryResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public string? JobTitle { get; set; }
    public int LinkedStudentCount { get; set; }
}
