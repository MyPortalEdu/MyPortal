using MyPortal.Contracts.Models.Agencies;

namespace MyPortal.Contracts.Models.School;

public class SchoolUpsertRequest : AgencyUpsertRequest
{
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