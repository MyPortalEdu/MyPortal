using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Bills")]
    public class Bill : AuditableEntity
    {
        public Guid StudentId { get; set; }

        public Guid? ChargeBillingPeriodId { get; set; }

        public DateTime DueDate { get; set; }

        public bool? IsDispatched { get; set; }

        public Student? Student { get; set; }

        public ChargeBillingPeriod? ChargeBillingPeriod { get; set; }
    }
}