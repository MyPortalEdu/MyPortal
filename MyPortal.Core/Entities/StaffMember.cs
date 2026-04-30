using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffMembers")]
    public class StaffMember : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PersonId { get; set; }
        
        public Guid? LineManagerId { get; set; }

        [Required] 
        [StringLength(128)] 
        public string Code { get; set; } = null!;

        [StringLength(50)]
        public string? BankName { get; set; }

        [StringLength(15)] 
        public string? BankAccount { get; set; }

        [StringLength(10)] 
        public string? BankSortCode { get; set; }

        [StringLength(9)] 
        public string? NiNumber { get; set; }
        
        [StringLength(128)]
        public string? Qualifications { get; set; }

        public bool IsTeachingStaff { get; set; }

        public bool IsDeleted { get; set; }

        public Person? Person { get; set; }

        public StaffMember? LineManager { get; set; }
        
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