using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PayScales")]
    public class PayScale : LookupEntity
    {
        public Guid ServiceTermId { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        // On a term with its own single spine this is just a window onto the term's points.
        // Otherwise the scale owns its points outright and generates them at PointInterval.
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MinimumPoint { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? MaximumPoint { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? PointInterval { get; set; }

        public ServiceTerm? ServiceTerm { get; set; }
    }
}
