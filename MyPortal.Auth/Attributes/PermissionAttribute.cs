using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;

namespace MyPortal.Auth.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    private const string Prefix = "perm:";
    
    public PermissionRequirement Requirement { get; }
    public string[] Permissions { get; }

    public PermissionAttribute(PermissionRequirement requirement, params string[] permissions)
    {
        Requirement = requirement;
        Permissions = permissions;
        Policy = $"{Prefix}{requirement}:{string.Join(',', Permissions)}";
    }
}