using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("GradeSets")]
    public class GradeSet : LookupEntity, ISystemEntity
    {
        [Required]
        [StringLength(256)]
        public required string Name { get; set; }
        
        public bool IsSystem { get; set; }
    }
}