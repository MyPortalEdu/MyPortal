using Microsoft.AspNetCore.Identity;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public UserType UserType { get; set; }
    public bool IsDefault { get; set; }
}