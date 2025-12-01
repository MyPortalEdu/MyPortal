using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("TrainingCourses")]
    public class TrainingCourse : LookupEntity
    {
        [Required]
        [StringLength(128)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;
    }
}