using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // Effective-dated: a rate change closes the current row and inserts a new one rather than
    // mutating in place, so history survives. Same convention as PayScalePointRate.
    [Table("SuperannuationSchemeRates")]
    public class SuperannuationSchemeRate : Entity
    {
        public Guid SuperannuationSchemeId { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal EmployerRate { get; set; }

        public SuperannuationScheme? SuperannuationScheme { get; set; }
    }
}
