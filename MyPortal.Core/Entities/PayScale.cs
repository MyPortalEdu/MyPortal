using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PayScales")]
    public class PayScale : LookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }
    }
}
