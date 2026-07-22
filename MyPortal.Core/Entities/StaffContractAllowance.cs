using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // An allowance paid on top of a contract's base salary (TLR1/2, SEN, R&R, London weighting, …).
    // A contract can carry several at once — MPS + TLR1 + SEN is a normal teacher package — each with
    // its own amount and period.
    [Table("StaffContractAllowances")]
    public class StaffContractAllowance : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffContractId { get; set; }

        // The allowance type (CBDS CS082 additional-payment category).
        public Guid AdditionalPaymentTypeId { get; set; }

        // Full-value annual amount of the allowance, before any pay factor.
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        // Proportion of the full amount actually paid (1.0000 = full). Null = full value.
        [Column(TypeName = "decimal(5,4)")]
        public decimal? PayFactor { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Counts towards pensionable pay.
        public bool IsSuperannuable { get; set; }

        // Subject to National Insurance.
        public bool IsSubjectToNi { get; set; }

        // Treated as a benefit in kind rather than cash pay.
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
