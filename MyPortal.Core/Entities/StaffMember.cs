using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffMembers")]
    public class StaffMember : Entity, ISoftDeleteEntity
    {
        public Guid? LineManagerId { get; set; }

        public Guid PersonId { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Code { get; set; }

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
    }
}