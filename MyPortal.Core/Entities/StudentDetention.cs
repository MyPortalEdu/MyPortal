using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("StudentDetentions")]
public class StudentDetention : Entity
{
    public Guid StudentId { get; set; }

    public Guid DetentionId { get; set; }

    public Guid? LinkedIncidentId { get; set; }

    public bool HasAttended { get; set; }

    public string? Notes { get; set; }

    public Student? Student { get; set; }
    public Detention? Detention { get; set; }
    public StudentIncident? LinkedIncident { get; set; }
}