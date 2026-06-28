namespace MyPortal.Contracts.Models.School;

public class SchoolDetailsResponse
{
    public Guid Id { get; set; }

    // Agency
    public Guid AgencyId { get; set; }
    public string Name { get; set; } = "";
    public string? Website { get; set; }
    public Guid AgencyTypeId { get; set; }

    // School bits
    public string Urn { get; set; } = "";
    public string Uprn { get; set; } = "";
    public int EstablishmentNumber { get; set; }

    public Guid? LocalAuthorityId { get; set; }
    public string? LocalAuthorityName { get; set; }

    public Guid SchoolPhaseId { get; set; }
    public string Phase { get; set; } = "";

    public Guid SchoolTypeId { get; set; }
    public string Type { get; set; } = "";

    public Guid GovernanceTypeId { get; set; }

    public Guid IntakeTypeId { get; set; }

    public Guid? HeadTeacherId { get; set; }
    public string? HeadTeacherFullName { get; set; }

    // UK Provider Reference Number.
    public string? Ukprn { get; set; }

    // Pay zone (basis for staff statutory salaries).
    public Guid? PayZoneId { get; set; }
    public string? PayZoneName { get; set; }

    // Statutory age range.
    public int? LowestAge { get; set; }
    public int? HighestAge { get; set; }

    // Net capacity + assessment date.
    public int? NetCapacity { get; set; }
    public DateTime? NetCapacityAssessmentDate { get; set; }

    // Special school facts.
    public bool IsSpecialSchool { get; set; }
    public Guid? SpecialSchoolOrganisationId { get; set; }
    public string? SpecialSchoolOrganisationName { get; set; }
    public Guid? SpecialSchoolTypeId { get; set; }
    public string? SpecialSchoolTypeName { get; set; }
    public int? MaxBoarders { get; set; }

    // Direct establishment contact.
    public string? Telephone { get; set; }
    public string? Email { get; set; }

    public bool IsLocal { get; set; }
}