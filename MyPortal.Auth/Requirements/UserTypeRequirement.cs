using Microsoft.AspNetCore.Authorization;

namespace MyPortal.Auth.Requirements;

public class UserTypeRequirement(string[] allowed) : IAuthorizationRequirement
{
    public string[] Allowed { get; } = allowed;
}