using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Posts")]
    public class Post : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        [Required]
        [StringLength(32)]
        public string Reference { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string Description { get; set; } = null!;

        public Guid? PostCategoryId { get; set; }

        public Guid? ServiceTermId { get; set; }

        [StringLength(10)]
        public string? SwrPostCode { get; set; }

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
