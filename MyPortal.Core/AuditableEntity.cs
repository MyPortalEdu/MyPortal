using System.ComponentModel.DataAnnotations;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core;

public class AuditableEntity : Entity, IAuditableEntity
{
    public Guid CreatedById { get; set; }
    
    [Required, StringLength(45)]
    public required string CreatedByIpAddress { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid LastModifiedById { get; set; }
    
    [Required, StringLength(40)]
    public required string LastModifiedByIpAddress { get; set; }
    
    public DateTime LastModifiedAt { get; set; }
    
    public User? CreatedBy { get; set; }
    public User? LastModifiedBy { get; set; }
}