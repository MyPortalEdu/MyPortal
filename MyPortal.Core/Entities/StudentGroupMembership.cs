using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentGroupMemberships")]
    public class StudentGroupMembership : Entity
    {
        public Guid StudentId { get; set; }

        public Guid StudentGroupId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public Student? Student { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}