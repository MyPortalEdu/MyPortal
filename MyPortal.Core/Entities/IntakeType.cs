using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("IntakeTypes")]
    public class IntakeType : LookupEntity, IOrderedLookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        public int DisplayOrder { get; set; }
    }
}
