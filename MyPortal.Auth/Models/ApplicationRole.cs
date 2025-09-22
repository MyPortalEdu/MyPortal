using Microsoft.AspNetCore.Identity;

namespace MyPortal.Auth.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}