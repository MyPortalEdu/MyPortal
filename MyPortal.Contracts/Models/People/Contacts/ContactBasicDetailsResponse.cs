namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// Read payload for the Basic details area of the contact profile. Carries person bio (excluding
/// cultural/medical-sensitive fields — ethnicity, NHS no. etc. live in their own areas) plus the
/// Contact-row fields (parental ballot, place of work, job title, NI number).
/// </summary>
public class ContactBasicDetailsResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }

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
