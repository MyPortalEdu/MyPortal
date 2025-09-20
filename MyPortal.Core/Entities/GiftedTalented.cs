using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("GiftedTalentedStudents")]
    public class GiftedTalented : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SubjectId { get; set; }

        [Required] 
        public string? Notes { get; set; }

        public Student? Student { get; set; }
        public Subject? Subject { get; set; }
    }
}