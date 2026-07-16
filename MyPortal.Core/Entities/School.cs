using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Schools")]
    public class School : Entity
    {
        public Guid AgencyId { get; set; }

        public Guid? LocalAuthorityId { get; set; }

        public int EstablishmentNumber { get; set; }

        [Required]
        [StringLength(128)]
        public string Urn { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string Uprn { get; set; } = null!;

        public Guid SchoolPhaseId { get; set; }

        public Guid SchoolTypeId { get; set; }

        public Guid GovernanceTypeId { get; set; }

        public Guid IntakeTypeId { get; set; }

        public Guid? HeadTeacherId { get; set; }

        public Guid? PayZoneId { get; set; }

        // UK Provider Reference Number (8 digits) — distinct from URN and the UPRN property reference.
        [StringLength(8)]
        public string? Ukprn { get; set; }

        // Statutory age range (lowest / highest national-curriculum ages admitted).
        public int? LowestAge { get; set; }

        public int? HighestAge { get; set; }

        public int? NetCapacity { get; set; }

        public DateTime? NetCapacityAssessmentDate { get; set; }

        // Special-school establishment facts (CBDS CS038 / CS077); gated by IsSpecialSchool.
        public bool IsSpecialSchool { get; set; }

        public Guid? SpecialSchoolOrganisationId { get; set; }

        public Guid? SpecialSchoolTypeId { get; set; }

        // Maximum boarding pupils (boarding establishments).
        public int? MaxBoarders { get; set; }

        // Direct establishment contact (the school currently inherits this from its Agency).
        [StringLength(30)]
        public string? Telephone { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        public bool IsLocal { get; set; }

        public Agency? Agency { get; set; }
        public SchoolPhase? SchoolPhase { get; set; }
        public SchoolType? SchoolType { get; set; }
        public GovernanceType? GovernanceType { get; set; }
        public IntakeType? IntakeType { get; set; }
        public Person? HeadTeacher { get; set; }
        public LocalAuthority? LocalAuthority { get; set; }
        public PayZone? PayZone { get; set; }
        public SpecialSchoolOrganisation? SpecialSchoolOrganisation { get; set; }
        public SpecialSchoolType? SpecialSchoolType { get; set; }
    }
}