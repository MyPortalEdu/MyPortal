using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities;

[Table("ResultSetReleases")]
public class ResultSetRelease : Entity
{
    public Guid ResultSetId { get; set; }
    public Guid StudentGroupId { get; set; }
    public DateTime ReleasedAt { get; set; }
    
    public ResultSet? ResultSet { get; set; }
    public StudentGroup? StudentGroup { get; set; }
}