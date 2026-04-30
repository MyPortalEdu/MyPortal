using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Classes")]
    public class Class : Entity, IAuditableEntity, IDirectoryEntity, IVersionedEntity
    {
        public Guid CourseId { get; set; }

        public Guid CurriculumGroupId { get; set; }

        public Guid DirectoryId { get; set; }

        [Required, StringLength(10)]
        public string Code { get; set; } = null!;

        public Course? Course { get; set; }
        public CurriculumGroup? Group { get; set; }
        public Directory? Directory { get; set; }
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}