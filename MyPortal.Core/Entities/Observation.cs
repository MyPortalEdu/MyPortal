using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Observations")]
    public class Observation : Entity
    {
        public DateTime Date { get; set; }

        public Guid ObserveeId { get; set; }

        public Guid ObserverId { get; set; }

        public Guid OutcomeId { get; set; }

        // The lesson focus / theme of the observation (e.g. questioning, differentiation).
        [StringLength(128)]
        public string? Focus { get; set; }

        // The class or subject observed (free text).
        [StringLength(128)]
        public string? SubjectObserved { get; set; }

        public string? Strengths { get; set; }

        public string? AreasForDevelopment { get; set; }

        public string? Notes { get; set; }

        public StaffMember? Observee { get; set; }

        public StaffMember? Observer { get; set; }

        public ObservationOutcome? Outcome { get; set; }
    }
}