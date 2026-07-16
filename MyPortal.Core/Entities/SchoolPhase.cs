using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("SchoolPhases")]
    public class SchoolPhase : LookupEntity, IOrderedLookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        public int DisplayOrder { get; set; }
    }
}
