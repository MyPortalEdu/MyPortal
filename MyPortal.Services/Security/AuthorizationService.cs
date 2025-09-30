using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;

namespace MyPortal.Services.Security;

public class AuthorizationService : IAuthorizationService
{
    private readonly ICurrentUser _user;
    private readonly IPermissionService _perms;

    public AuthorizationService(ICurrentUser user, IPermissionService perms)
        => (_user, _perms) = (user, perms);

    public async Task RequireAsync(string permission, CancellationToken ct = default)
    {
        var id = _user.UserId ?? throw new PermissionException("Not authenticated.");
        if (!await _perms.HasAsync(id, permission, ct))
            throw new PermissionException($"Missing permission: {permission}");
    }
}