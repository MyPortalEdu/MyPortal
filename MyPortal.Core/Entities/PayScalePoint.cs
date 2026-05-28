using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PayScalePoints")]
    public class PayScalePoint : LookupEntity
    {
        public Guid PayScaleId { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public PayScale? PayScale { get; set; }
    }
}
