using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Auth.Requirements;

namespace MyPortal.Auth.Handlers;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _perms;
    private readonly ICurrentUser _user;

    public PermissionHandler(IPermissionService perms, ICurrentUser user)
        => (_perms, _user) = (perms, user);

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement req)
    {
        if (_user.UserId is null) return;

        if (req.Permissions.Length == 0)
        {
            // Misconfigured policy (no permissions specified) — fail closed.
            return;
        }

        var checks = new List<bool>(req.Permissions.Length);
        foreach (var p in req.Permissions)
            checks.Add(await _perms.HasPermissionAsync(p));

        bool ok = req.Mode == PermissionMode.RequireAny
            ? checks.Any(x => x)
            : checks.All(x => x);

        if (ok) context.Succeed(req);
    }
}