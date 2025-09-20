using MyPortal.Core.Entities;

namespace MyPortal.Core.Interfaces;

public interface IAuditableEntity : IEntity
{
    Guid CreatedById { get; set; }
    string CreatedByIpAddress { get; set; }
    DateTime CreatedAt { get; set; }
    Guid LastModifiedById { get; set; }
    string LastModifiedByIpAddress { get; set; }
    DateTime LastModifiedAt { get; set; }
    
    User? CreatedBy { get; set; }
    User? LastModifiedBy { get; set; }
}