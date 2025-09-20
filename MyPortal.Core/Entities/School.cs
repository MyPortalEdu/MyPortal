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
        public required string Urn { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Uprn { get; set; }

        public Guid SchoolPhaseId { get; set; }

        public Guid SchoolTypeId { get; set; }

        public Guid GovernanceTypeId { get; set; }

        public Guid IntakeTypeId { get; set; }

        public Guid? HeadTeacherId { get; set; }

        public bool IsLocal { get; set; }

        public Agency? Agency { get; set; }
        public SchoolPhase? SchoolPhase { get; set; }
        public SchoolType? SchoolType { get; set; }
        public GovernanceType? GovernanceType { get; set; }
        public IntakeType? IntakeType { get; set; }
        public Person? HeadTeacher { get; set; }
        public LocalAuthority? LocalAuthority { get; set; }
    }
}