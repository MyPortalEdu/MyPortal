using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
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
        var perms = await GetCurrentUserPermissionsAsync(ct);
        return permissions.Any(perms.Contains);
    }

    public async Task<bool> HasAllPermissionsAsync(string[] permissions, CancellationToken ct = default)
    {
        var perms = await GetCurrentUserPermissionsAsync(ct);
        return permissions.All(perms.Contains);
    }

    private static readonly IReadOnlySet<string> NoPermissions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlySet<string>> GetCurrentUserPermissionsAsync(CancellationToken ct = default)
    {
        // Short-circuits before the enabled check (which dereferences UserId) keep the
        // unauthenticated path from touching the status cache or provider.
        if (_user.UserId is null) return NoPermissions;

        if (!await IsCurrentUserEnabledAsync(ct)) return NoPermissions;

        var roles = await _user.GetRolesAsync(ct);
        var perms = await _provider.GetPermissionsForRolesAsync(roles, ct);

        // Case-insensitive set so callers get O(1), comparer-correct membership tests.
        return new HashSet<string>(perms, StringComparer.OrdinalIgnoreCase);
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

    public async Task<IList<PermissionResponse>> GetAllPermissionsAsync(UserType? userType, CancellationToken cancellationToken)
    {
        var perms = await _permissionRepository.GetListAsync(cancellationToken: cancellationToken);

        var filtered = userType.HasValue ? perms.Where(p => p.UserType == userType.Value) : perms;

        return filtered.Select(MapPermissionToDto).ToList();
    }

    public PermissionResponse MapPermissionToDto(Permission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name,
            FriendlyName = permission.FriendlyName,
            Area = permission.Area,
            UserType = permission.UserType
        };
    }
}