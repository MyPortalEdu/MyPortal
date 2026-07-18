using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;

namespace MyPortal.Auth.Requirements;

public class PermissionRequirement(PermissionMode mode, params string[] permissions) : IAuthorizationRequirement
{
    public string[] Permissions { get; } = permissions;
    public PermissionMode Mode { get; } = mode;
}