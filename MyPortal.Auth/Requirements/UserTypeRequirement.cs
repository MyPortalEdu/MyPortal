using Microsoft.AspNetCore.Authorization;

namespace MyPortal.Auth.Requirements;

public class UserTypeRequirement : IAuthorizationRequirement
{
    public string[] Allowed { get; }
    
    public UserTypeRequirement(string[] allowed)
    {
        Allowed = allowed;
    }
}