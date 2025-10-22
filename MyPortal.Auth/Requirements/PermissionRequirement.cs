using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;

namespace MyPortal.Auth.Requirements;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public PermissionMode Mode { get; }
    
    public PermissionRequirement(PermissionMode mode, params string[] permissions)
    {
        Permissions = permissions;
        Mode = mode;
    }
}