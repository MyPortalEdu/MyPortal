using MyPortal.Auth.Interfaces;

namespace MyPortal.Services.Security;

public class PermissionService : IPermissionService
{
    private readonly ICurrentUser _user;
    private readonly IRolePermissionProvider _provider;

    public PermissionService(ICurrentUser user, IRolePermissionProvider provider)
    {
        _user = user;
        _provider = provider;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        if (_user.UserId != userId) return false;
        var roles = await _user.GetRolesAsync(ct);
        var perms = await _provider.GetPermissionsForRolesAsync(roles, ct);
        return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}