using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SchoolPhases")]
    public class SchoolPhase : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; }
    }
}