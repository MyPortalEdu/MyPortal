using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("BulletinAcknowledgements")]
public class BulletinAcknowledgement : Entity
{
    public Guid BulletinId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AcknowledgedAt { get; set; }

    public Bulletin? Bulletin { get; set; }
    public User? User { get; set; }
}
