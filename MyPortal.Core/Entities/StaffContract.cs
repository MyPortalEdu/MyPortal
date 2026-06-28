using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffContracts")]
    public class StaffContract : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffEmploymentId { get; set; }

        public Guid ContractTypeId { get; set; }

        // DfE workforce-census staff role (CS050). PostTitle stays for display.
        public Guid? StaffRoleId { get; set; }

        public Guid? ServiceTermId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PayScaleId { get; set; }

        public Guid? PayScalePointId { get; set; }

        [Required]
        [StringLength(256)]
        public string PostTitle { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // FTE expressed as percentage 0.0000 – 1.0000 (e.g. 0.6000 for 60%).
        [Column(TypeName = "decimal(5,4)")]
        public decimal Fte { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? HoursPerWeek { get; set; }

        // Working weeks per year (TTO = ~39, all-year = 52). Null = not tracked.
        [Column(TypeName = "decimal(4,2)")]
        public decimal? WeeksPerYear { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? AnnualSalary { get; set; }

        // Agency / supply worker (workforce census).
        public bool IsAgencySupply { get; set; }

        // Pay protected at a higher safeguarded rate (workforce census pay collection).
        public bool SafeguardedSalary { get; set; }

        // Paid at a daily rate rather than annual salary (supply staff).
        public bool DailyRate { get; set; }

        public bool IsDeleted { get; set; }

        public StaffEmployment? StaffEmployment { get; set; }
        public ContractType? ContractType { get; set; }
        public StaffRole? StaffRole { get; set; }
        public ServiceTerm? ServiceTerm { get; set; }
        public Department? Department { get; set; }
        public PayScale? PayScale { get; set; }
        public PayScalePoint? PayScalePoint { get; set; }

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
