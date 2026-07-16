using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // Effective-dated statutory salary for a (PayScalePoint, PayZone) pair.
    // Annual pay awards close the current row (set EffectiveTo) and insert a
    // new one rather than mutating in place, so history stays intact.
    [Table("PayScalePointRates")]
    public class PayScalePointRate : Entity
    {
        public Guid PayScalePointId { get; set; }

        public Guid PayZoneId { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AnnualSalary { get; set; }

        public PayScalePoint? PayScalePoint { get; set; }
        public PayZone? PayZone { get; set; }
    }
}
