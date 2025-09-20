using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("IntakeTypes")]
    public class IntakeType : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }
    }
}