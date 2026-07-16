using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PayZones")]
    public class PayZone : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;
    }
}
