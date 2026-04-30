using Microsoft.AspNetCore.Identity;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
    
    public bool IsSystem { get; set; }
    
    // Audit
    public Guid? CreatedById { get; set; }
    public string CreatedByIpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? LastModifiedById { get; set; }
    public string LastModifiedByIpAddress { get; set; } = string.Empty;
    public DateTime LastModifiedAt { get; set; }
    public long Version { get; set; }
}