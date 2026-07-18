namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A Person surfaced by the general person search (used to link a Person to a user account).
/// Name parts + DOB are enough to disambiguate candidates.
/// </summary>
public class PersonSearchResponse
{
    public Guid PersonId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public DateTime? Dob { get; set; }
}
