using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Subjects")]
    public class Subject : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid SubjectCodeId { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(5)]
        public string Code { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public SubjectCode? SubjectCode { get; set; }
        
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