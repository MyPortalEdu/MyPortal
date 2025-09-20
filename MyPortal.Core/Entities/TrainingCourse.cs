using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("TrainingCourses")]
    public class TrainingCourse : LookupEntity
    {
        [Required]
        [StringLength(128)]
        public required string Code { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }
    }
}