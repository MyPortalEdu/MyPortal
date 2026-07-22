using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A child protection plan (formerly child protection register). Dated; a safeguarding record,
    // not a census item.
    [Table("StudentChildProtectionPlans")]
    public class StudentChildProtectionPlan : Entity
    {
        public Guid StudentId { get; set; }

        // The local authority responsible for the plan.
        public Guid? LocalAuthorityId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1024)]
        public string? Comment { get; set; }

        public Student? Student { get; set; }
        public LocalAuthority? LocalAuthority { get; set; }
    }
}
