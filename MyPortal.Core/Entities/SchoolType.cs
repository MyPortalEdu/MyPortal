using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SchoolTypes")]
    public class SchoolType : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }
    }
}