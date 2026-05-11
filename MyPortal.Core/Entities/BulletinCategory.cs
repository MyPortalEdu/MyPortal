using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities;

[Table("BulletinCategories")]
public class BulletinCategory : Entity, IAuditableEntity, ISystemEntity, IVersionedEntity
{
    [Required, StringLength(50)]
    public string Name { get; set; } = null!;

    [Required, StringLength(50)]
    public string Icon { get; set; } = null!;

    [Required, StringLength(9)]
    public string ColourCode { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public bool Active { get; set; } = true;

    public bool IsSystem { get; set; }

    public Guid CreatedById { get; set; }
    public string CreatedByIpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid LastModifiedById { get; set; }
    public string LastModifiedByIpAddress { get; set; } = string.Empty;
    public DateTime LastModifiedAt { get; set; }
    public User? CreatedBy { get; set; }
    public User? LastModifiedBy { get; set; }
    public long Version { get; set; }
}
