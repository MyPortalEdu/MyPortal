using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffContractAllowances")]
    public class StaffContractAllowance : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffContractId { get; set; }

        public Guid AdditionalPaymentTypeId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? PayFactor { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsSuperannuable { get; set; }

        public bool IsSubjectToNi { get; set; }

        public bool IsBenefitInKind { get; set; }

        [StringLength(256)]
        public string? Reason { get; set; }

        public bool IsDeleted { get; set; }

        public StaffContract? StaffContract { get; set; }
        public AdditionalPaymentType? AdditionalPaymentType { get; set; }

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
