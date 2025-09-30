using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;

namespace MyPortal.Auth.Handlers;

public sealed class UserTypeHandler : AuthorizationHandler<UserTypeRequirementModel>
{
    private readonly ICurrentUser _me;
    public UserTypeHandler(ICurrentUser me) => _me = me;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserTypeRequirementModel req)
    {
        var ut = _me.UserType.ToString();
        if (req.Allowed.Length == 0 || req.Allowed.Contains(ut, StringComparer.OrdinalIgnoreCase))
            context.Succeed(req);

        return Task.CompletedTask;
    }
}