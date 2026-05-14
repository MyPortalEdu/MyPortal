using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;

namespace MyPortal.Services.Security;

public class PermissionService : IPermissionService
{
    private readonly ICurrentUser _user;
    private readonly IRolePermissionProvider _provider;
    private readonly IPermissionRepository _permissionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStatusCache _userStatusCache;

    public PermissionService(ICurrentUser user, IRolePermissionProvider provider,
        IPermissionRepository permissionRepository, UserManager<ApplicationUser> userManager,
        IUserStatusCache userStatusCache)
    {
        _user = user;
        _provider = provider;
        _permissionRepository = permissionRepository;
        _userManager = userManager;
        _userStatusCache = userStatusCache;
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        return await HasAllPermissionsAsync([permission], ct);
    }

    public async Task<bool> HasAnyPermissionsAsync(string[] permissions, CancellationToken ct = default)
    {
        if (_user.UserId is null) return false;
        
        if (!await IsCurrentUserEnabledAsync(ct)) return false;

        var roles = await _user.GetRolesAsync(ct);
        var perms = await _provider.GetPermissionsForRolesAsync(roles, ct);
        return perms.Any(x => permissions.Contains(x, StringComparer.OrdinalIgnoreCase));
    }

    public async Task<bool> HasAllPermissionsAsync(string[] permissions, CancellationToken ct = default)
    {
        if (_user.UserId is null) return false;
        
        if (!await IsCurrentUserEnabledAsync(ct)) return false;

        var roles = await _user.GetRolesAsync(ct);
        var perms = await _provider.GetPermissionsForRolesAsync(roles, ct);
        return permissions.All(x => perms.Contains(x, StringComparer.OrdinalIgnoreCase));
    }

    private Task<bool> IsCurrentUserEnabledAsync(CancellationToken ct)
    {
        var userId = _user.UserId!.Value;
        return _userStatusCache.IsEnabledAsync(userId, async _ =>
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user?.IsEnabled ?? false;
        }, ct);
    }

    public async Task<IList<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var perms = await _permissionRepository.GetListAsync(cancellationToken: cancellationToken);

        return perms.Select(MapPermissionToDto).ToList();
    }

    public PermissionResponse MapPermissionToDto(Permission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name,
            FriendlyName = permission.FriendlyName,
            Area = permission.Area
        };
    }
}