namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Basic details area, also used to create a contact. Touches basic person
/// bio plus the Contact-row fields (parental ballot, place of work, job title, NI number). Cultural
/// / medical fields require their own area endpoints; this payload has no way to overwrite them.
/// </summary>
public class ContactBasicDetailsUpsertRequest
{
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public Guid? PhotoId { get; set; }
    public string Gender { get; set; } = null!;
    public DateTime? Dob { get; set; }
    public DateTime? Deceased { get; set; }

    public bool ParentalBallot { get; set; }
    public string? PlaceOfWork { get; set; }
    public string? JobTitle { get; set; }
    public string? NiNumber { get; set; }
}
