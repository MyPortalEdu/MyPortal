using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("CurriculumGroupSessions")]
public class CurriculumGroupSession : Entity
{
    public Guid CurriculumGroupId { get; set; }

    public Guid SubjectId { get; set; }

    public Guid SessionTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int SessionAmount { get; set; }

    public CurriculumGroup? CurriculumGroup { get; set; }
    public Subject? Subject { get; set; }
    public SessionType? SessionType { get; set; }
}