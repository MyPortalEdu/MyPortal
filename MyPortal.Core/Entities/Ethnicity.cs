using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Ethnicities")]
    public class Ethnicity : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;
    }
}