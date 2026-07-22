using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // An established position in the school's staffing structure — the thing a contract is held
    // against, and the unit a vacancy is reported for. School-level reference data maintained in
    // Staff Setup, not created ad-hoc from a staff record.
    [Table("Posts")]
    public class Post : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        // Short unique identifier for the post within the school (e.g. "TCH-MFL-02").
        [Required]
        [StringLength(32)]
        public string Reference { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string Description { get; set; } = null!;

        public Guid? PostCategoryId { get; set; }

        public Guid? ServiceTermId { get; set; }

        // School Workforce Return post code, for census mapping.
        [StringLength(10)]
        public string? SwrPostCode { get; set; }

        // FTE the post is funded/established for — the basis for comparing establishment to actual.
        [Column(TypeName = "decimal(5,4)")]
        public decimal? EstablishedFte { get; set; }

        public bool IsDeleted { get; set; }

        public PostCategory? PostCategory { get; set; }
        public ServiceTerm? ServiceTerm { get; set; }

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
