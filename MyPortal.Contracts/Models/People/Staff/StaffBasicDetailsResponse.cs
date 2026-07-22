namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Read payload for the Basic details area of the staff profile. Carries person bio (excluding
/// equality-sensitive fields like NHS no. and ethnicity — those live in the EqualityDetails area)
/// plus the staff identifier. Other staff fields belong to their own areas
/// (Employment / Professional / etc.).
/// </summary>
public class StaffBasicDetailsResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Code { get; set; } = null!;

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
