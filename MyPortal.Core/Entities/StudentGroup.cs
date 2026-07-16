using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StudentGroups")]
    public class StudentGroup : LookupEntity, IAuditableEntity, ISystemEntity, IAcademicYearEntity, IVersionedEntity
    {
        [Required] [StringLength(10)] 
        public string Code { get; set; } = null!;
        
        public Guid AcademicYearId { get; set; }

        public Guid? PromoteToGroupId { get; set; }

        public Guid? MainSupervisorId { get; set; }

        public int? MaxMembers { get; set; }
        
        [StringLength(256)]
        public string? Notes { get; set; }

        public bool IsSystem { get; set; }

        public AcademicYear? AcademicYear { get; set; }
        public StudentGroup? PromoteToGroup { get; set; }
        public StudentGroupSupervisor? MainSupervisor { get; set; }
        
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