using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PayScalePoints")]
    public class PayScalePoint : LookupEntity
    {
        // Exactly one owner: the scale when it has its own range, the service term when the
        // term runs a single spine that its scales are windows onto.
        public Guid? PayScaleId { get; set; }

        public Guid? ServiceTermId { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = null!;

        // The spine position (1.0, 1.5, 2.0). Generation matches on this, so regenerating a
        // scale leaves existing points — and the contracts pointing at them — alone.
        [Column(TypeName = "decimal(6,2)")]
        public decimal PointValue { get; set; }

        public PayScale? PayScale { get; set; }
        public ServiceTerm? ServiceTerm { get; set; }
    }
}
