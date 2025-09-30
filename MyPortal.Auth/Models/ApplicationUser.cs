using Microsoft.AspNetCore.Identity;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }

    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
}