using System.ComponentModel.DataAnnotations;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core;

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public Guid CreatedById { get; set; }

    [Required, StringLength(45)] 
    public string CreatedByIpAddress { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; }
    
    public Guid LastModifiedById { get; set; }

    [Required, StringLength(40)] 
    public string LastModifiedByIpAddress { get; set; } = default!;
    
    public DateTime LastModifiedAt { get; set; }
    
    public User? CreatedBy { get; set; }
    public User? LastModifiedBy { get; set; }
}