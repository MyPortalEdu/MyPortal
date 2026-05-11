using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;

namespace MyPortal.Core.Entities;

[Table("BulletinAudiences")]
public class BulletinAudience : Entity
{
    public Guid BulletinId { get; set; }

    public BulletinAudienceKind AudienceKind { get; set; }

    // Required iff AudienceKind == StudentGroup. The DB has a CHECK constraint
    // enforcing this — application code should match.
    public Guid? StudentGroupId { get; set; }

    public Bulletin? Bulletin { get; set; }
    public StudentGroup? StudentGroup { get; set; }
}
