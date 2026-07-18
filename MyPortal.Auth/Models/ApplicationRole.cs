using Microsoft.AspNetCore.Identity;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsSystem { get; set; }

    // Portal audience. A role may only hold permissions of the same UserType, and may only be
    // assigned to users of the same UserType. Set at create/seed time; immutable thereafter.
    public UserType UserType { get; set; }

    // Seeded default role: identity protected (no delete/rename) but permission grants editable.
    // Distinct from IsSystem, which locks the row entirely.
    public bool IsDefault { get; set; }
}