namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Basic details area. Touches only basic person bio + the staff code.
/// Equality, employment, and professional fields require their own area endpoints — this payload
/// has no way to overwrite them.
/// </summary>
public class StaffBasicDetailsUpsertRequest
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

    public string Code { get; set; } = null!;
}
