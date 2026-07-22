namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Write payload for the Equality &amp; Diversity area. Touches only the person's equality
/// single-selects and the staff disability declaration/specifics — no other area's fields are
/// reachable from here.
/// </summary>
public class StaffEqualityDetailsUpsertRequest
{
    public Guid? EthnicityId { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? FirstLanguageId { get; set; }
    public Guid? MaritalStatusId { get; set; }
    public Guid? ReligionId { get; set; }
    public Guid? SexualOrientationId { get; set; }
    public Guid? GenderIdentityId { get; set; }

    public bool HasDisability { get; set; }
    public string? DisabilityDetails { get; set; }
    public List<Guid> DisabilityIds { get; set; } = [];
}
