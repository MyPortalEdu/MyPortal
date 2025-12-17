using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudyTopics")]
    public class StudyTopic : LookupEntity
    {
        public Guid CourseId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;

        public int Order { get; set; }

        public Course? Course { get; set; }
    }
}