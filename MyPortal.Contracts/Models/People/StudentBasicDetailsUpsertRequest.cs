namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Basic details area, also used to create a student. Touches only basic
/// person bio — the admission number is auto-assigned on create and read-only thereafter, and
/// registration / cultural / medical fields require their own area endpoints. This payload has no
/// way to overwrite them.
/// </summary>
public class StudentBasicDetailsUpsertRequest
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
}
