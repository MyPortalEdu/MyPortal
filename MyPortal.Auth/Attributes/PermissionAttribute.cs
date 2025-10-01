using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;

namespace MyPortal.Auth.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    private const string Prefix = "perm:";
    
    public PermissionMode Mode { get; }
    public string[] Permissions { get; }

    public PermissionAttribute(PermissionMode mode, params string[] permissions)
    {
        Mode = mode;
        Permissions = permissions;
        Policy = $"{Prefix}{mode}:{string.Join(',', Permissions)}";
    }
}