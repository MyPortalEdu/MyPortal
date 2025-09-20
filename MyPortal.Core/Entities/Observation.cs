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

        public string? Notes { get; set; }

        public StaffMember? Observee { get; set; }

        public StaffMember? Observer { get; set; }

        public ObservationOutcome? Outcome { get; set; }
    }
}