using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Ethnicities")]
    public class Ethnicity : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }
    }
}