using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenStatus")]
    public class SenStatus : LookupEntity
    {
        [Required]
        [StringLength(1)]
        public required string Code { get; set; }
    }
}