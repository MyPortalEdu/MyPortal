using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("RegGroups")]
    public class RegGroup : Entity, IAuditableEntity, IStudentGroupEntity, IVersionedEntity
    {
        public Guid StudentGroupId { get; set; }

        public Guid YearGroupId { get; set; }

        public Guid? RoomId { get; set; }

        public StudentGroup? StudentGroup { get; set; }

        public YearGroup? YearGroup { get; set; }
        
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