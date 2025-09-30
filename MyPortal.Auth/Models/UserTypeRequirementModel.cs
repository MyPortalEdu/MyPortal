using Microsoft.AspNetCore.Authorization;

namespace MyPortal.Auth.Models;

public class UserTypeRequirementModel : IAuthorizationRequirement
{
    public string[] Allowed { get; }
    
    public UserTypeRequirementModel(string[] allowed)
    {
        Allowed = allowed;
    }
}