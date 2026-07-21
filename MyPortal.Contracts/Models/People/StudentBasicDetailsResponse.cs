namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Basic details area of the student profile. Carries person bio (excluding
/// cultural/medical-sensitive fields — ethnicity, NHS no. etc. live in their own areas) plus the
/// admission number. Enrolment, UPN/ULN and pastoral placement belong to the Registration area.
/// </summary>
public class StudentBasicDetailsResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public int AdmissionNumber { get; set; }

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
}
