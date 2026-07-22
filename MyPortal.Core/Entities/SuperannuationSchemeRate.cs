using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // Effective-dated employer contribution rate for a scheme. Rate changes close the current row
    // (set EffectiveTo) and insert a new one rather than mutating in place, so history stays intact
    // — same shape as PayScalePointRate.
    [Table("SuperannuationSchemeRates")]
    public class SuperannuationSchemeRate : Entity
    {
        public Guid SuperannuationSchemeId { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        // Employer contribution as a percentage of pensionable pay (e.g. 28.68 for TPS).
        [Column(TypeName = "decimal(5,2)")]
        public decimal EmployerRate { get; set; }

        public SuperannuationScheme? SuperannuationScheme { get; set; }
    }
}
