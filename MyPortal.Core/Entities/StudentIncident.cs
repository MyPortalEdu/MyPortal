using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("StudentIncidents")]
public class StudentIncident : Entity
{ 
    public Guid StudentId { get; set; }

    public Guid IncidentId { get; set; }

    public Guid RoleTypeId { get; set; }

    public Guid? OutcomeId { get; set; }

    public Guid StatusId { get; set; }

    public int Points { get; set; }

    public Student? Student { get; set; }
    public Incident? Incident { get; set; }
    public BehaviourRoleType? RoleType { get; set; }
    public BehaviourOutcome? Outcome { get; set; }
    public BehaviourStatus? Status { get; set; }
}