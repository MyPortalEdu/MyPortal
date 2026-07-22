using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffContractSalaryChanges")]
    public class StaffContractSalaryChange : Entity, IAuditableEntity
    {
        public Guid StaffContractId { get; set; }

        public Guid? OldPayScalePointId { get; set; }

        public Guid? NewPayScalePointId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OldAnnualSalary { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? NewAnnualSalary { get; set; }

        public StaffContract? StaffContract { get; set; }

        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
    }
}
