using Microsoft.AspNetCore.Identity;

namespace MyPortal.Auth.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }

    public Guid? PersonId { get; set; }
        
    public int UserType { get; set; }

    public bool IsEnabled { get; set; }
}