namespace MyPortal.Contracts.Models.School;

public class SchoolUpsertRequest
{
    public string Name { get; set; } = null!;

    public string? Website { get; set; }

    public long ExpectedVersion { get; set; }

    public Guid? LocalAuthorityId { get; set; }

    public int EstablishmentNumber { get; set; }

    public string Urn { get; set; } = null!;

    public string Uprn { get; set; } = null!;

    public Guid SchoolPhaseId { get; set; }

    public Guid SchoolTypeId { get; set; }

    public Guid GovernanceTypeId { get; set; }

    public Guid IntakeTypeId { get; set; }

    public Guid? HeadTeacherId { get; set; }

    // UK Provider Reference Number (8 digits).
    public string? Ukprn { get; set; }

    // Pay zone — drives statutory salary lookups for staff contracts.
    public Guid? PayZoneId { get; set; }

    // Statutory age range admitted.
    public int? LowestAge { get; set; }

    public int? HighestAge { get; set; }

    // Net capacity + the date it was assessed.
    public int? NetCapacity { get; set; }

    public DateTime? NetCapacityAssessmentDate { get; set; }

    // Special-school establishment facts (gated by IsSpecialSchool).
    public bool IsSpecialSchool { get; set; }

    public Guid? SpecialSchoolOrganisationId { get; set; }

    public Guid? SpecialSchoolTypeId { get; set; }

    public int? MaxBoarders { get; set; }

    // Direct establishment contact.
    public string? Telephone { get; set; }

    public string? Email { get; set; }
}
