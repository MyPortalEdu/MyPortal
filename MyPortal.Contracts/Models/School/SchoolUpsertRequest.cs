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
}
