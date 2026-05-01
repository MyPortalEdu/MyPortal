using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Auth.Requirements;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Handlers;

public sealed class UserTypeHandler : AuthorizationHandler<UserTypeRequirement>
{
    private readonly ICurrentUser _me;
    public UserTypeHandler(ICurrentUser me) => _me = me;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserTypeRequirement req)
    {
        if (_me.UserType == UserType.Unknown)
        {
            return Task.CompletedTask;
        }

        if (req.Allowed.Length == 0)
        {
            // Fail closed: a [UserType] attribute with no allowed types is a misconfiguration,
            // not a wildcard. Don't grant access.
            return Task.CompletedTask;
        }

        var ut = _me.UserType.ToString();
        if (req.Allowed.Contains(ut, StringComparer.OrdinalIgnoreCase))
            context.Succeed(req);

        return Task.CompletedTask;
    }
}