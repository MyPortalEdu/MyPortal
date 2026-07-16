using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Departments")]
    public class Department : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        [StringLength(9)]
        public string? ColourCode { get; set; }

        public int DisplayOrder { get; set; }
    }
}
