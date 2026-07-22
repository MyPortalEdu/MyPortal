using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A Personal Education Plan for a looked-after child. Dated, with a set of contributors
    // (the designated teacher and others who produce the plan).
    [Table("StudentPeps")]
    public class StudentPep : Entity
    {
        public Guid StudentId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1024)]
        public string? Comment { get; set; }

        public Student? Student { get; set; }
        public ICollection<StudentPepContributor>? Contributors { get; set; }
    }
}
