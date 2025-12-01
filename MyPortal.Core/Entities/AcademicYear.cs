using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AcademicYears")]
    public class AcademicYear : Entity
    {
        [Required] 
        [StringLength(128)] 
        public string Name { get; set; } = null!;

        public bool IsLocked { get; set; }
    }
}