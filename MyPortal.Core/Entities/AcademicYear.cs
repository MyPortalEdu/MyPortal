using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AcademicYears")]
    public class AcademicYear : Entity
    {
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }

        public bool IsLocked { get; set; }
    }
}