using Microsoft.AspNetCore.Authorization;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;

namespace MyPortal.Auth.Handlers;

public class PermissionHandler : AuthorizationHandler<PermissionRequirementModel>
{
    private readonly IPermissionService _perms;
    private readonly ICurrentUser _user;

    public PermissionHandler(IPermissionService perms, ICurrentUser user)
        => (_perms, _user) = (perms, user);

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirementModel req)
    {
        if (_user.UserId is null) return;

        var userId = _user.UserId.Value;

        if (req.Permissions.Length == 0)
        {
            context.Succeed(req);
            return;
        }

        var checks = new List<bool>(req.Permissions.Length);
        foreach (var p in req.Permissions)
            checks.Add(await _perms.HasPermissionAsync(userId, p));

        bool ok = req.Mode == PermissionRequirement.Any
            ? checks.Any(x => x)
            : checks.All(x => x);

        if (ok) context.Succeed(req);
    }
}