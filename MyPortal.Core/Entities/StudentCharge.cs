using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentCharges")]
    public class StudentCharge : Entity
    {
        public Guid StudentId { get; set; }

        public Guid ChargeId { get; set; }

        public Guid ChargeBillingPeriodId { get; set; }

        public string? Description { get; set; }

        public Student? Student { get; set; }
        public Charge? Charge { get; set; }
        public ChargeBillingPeriod? ChargeBillingPeriod { get; set; }
    }
}