using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("People")]
    public class Person : Entity, IAuditableEntity, IDirectoryEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid DirectoryId { get; set; }
        
        [StringLength(128)]
        public string? Title { get; set; }
        
        [StringLength(256)]
        public string? PreferredFirstName { get; set; }
        
        [StringLength(256)]
        public string? PreferredLastName { get; set; }

        [Required]
        [StringLength(256)]
        public string FirstName { get; set; } = null!;
        
        [StringLength(256)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(256)]
        public string LastName { get; set; } = null!;

        // Former / previous surname (CTF & census). Distinct from the preferred name.
        [StringLength(256)]
        public string? FormerSurname { get; set; }

        public Guid? PhotoId { get; set; }
        
        [StringLength(10)]
        public string? NhsNumber { get; set; }

        [Required]
        [StringLength(1)]
        public string Gender { get; set; } = null!;
        
        public DateTime? Dob { get; set; }
        
        public DateTime? Deceased { get; set; }

        public Guid? EthnicityId { get; set; }

        public Guid? NationalityId { get; set; }

        public Guid? FirstLanguageId { get; set; }

        public Guid? MaritalStatusId { get; set; }

        public Guid? ReligionId { get; set; }

        public Guid? SexualOrientationId { get; set; }

        public Guid? GenderIdentityId { get; set; }

        // Proficiency in English / EAL stage (CBDS CS089) + the date assessed.
        public Guid? EnglishProficiencyId { get; set; }

        public DateTime? EnglishProficiencyDate { get; set; }

        // At-a-glance flag that the person has medical needs (detail in PersonConditions).
        public bool HasMedicalNeeds { get; set; }

        public bool IsDeleted { get; set; }

        public Photo? Photo { get; set; }
        public Ethnicity? Ethnicity { get; set; }
        public Nationality? Nationality { get; set; }
        public Language? FirstLanguage { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public Religion? Religion { get; set; }
        public SexualOrientation? SexualOrientation { get; set; }
        public GenderIdentity? GenderIdentity { get; set; }
        public EnglishProficiency? EnglishProficiency { get; set; }
        public Directory? Directory { get; set; }
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}