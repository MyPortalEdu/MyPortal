using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;

namespace MyPortal.Auth.Models;

public class PermissionRequirementModel : IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public PermissionRequirement Mode { get; }
    
    public PermissionRequirementModel(PermissionRequirement mode, params string[] permissions)
    {
        Permissions = permissions;
        Mode = mode;
    }
}