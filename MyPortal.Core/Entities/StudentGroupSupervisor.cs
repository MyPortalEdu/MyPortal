using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentGroupSupervisors")]
    public class StudentGroupSupervisor : Entity
    {
        public Guid StudentGroupId { get; set; }

        public Guid SupervisorId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public StudentGroup? StudentGroup { get; set; }
        public StaffMember? Supervisor { get; set; }
    }
}