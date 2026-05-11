using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("BulletinAudienceAllowedGroups")]
public class BulletinAudienceAllowedGroup
{
    public Guid StudentGroupId { get; set; }

    public StudentGroup? StudentGroup { get; set; }
}
