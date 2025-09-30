namespace MyPortal.Contracts.Schools.Queries;

public class SchoolDetailsDto
{
    public Guid Id { get; set; }

    // Agency
    public Guid AgencyId { get; set; }
    public string Name { get; set; } = "";
    public string? Website { get; set; }
    public string AgencyType { get; set; } = "";

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
    public string GovernanceType { get; set; } = "";

    public Guid IntakeTypeId { get; set; }
    public string IntakeType { get; set; } = "";

    public Guid? HeadTeacherId { get; set; }
    public string? HeadTeacherFullName { get; set; }

    public bool IsLocal { get; set; }
}