using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffQualifications")]
    public class StaffQualification : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid? QualificationLevelId { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; } = null!;

        [StringLength(256)]
        public string? Subject { get; set; }

        [StringLength(256)]
        public string? AwardingBody { get; set; }

        [StringLength(20)]
        public string? Grade { get; set; }

        // Class of degree (CBDS CS062) where the qualification is a degree — structures Grade.
        public Guid? ClassOfDegreeId { get; set; }

        public int? YearAwarded { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public QualificationLevel? QualificationLevel { get; set; }
        public ClassOfDegree? ClassOfDegree { get; set; }

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
