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
        public string Name { get; set; } = null!;
        
        public bool IsSystem { get; set; }
    }
}