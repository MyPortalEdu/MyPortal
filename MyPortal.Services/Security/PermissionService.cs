using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Services;

namespace MyPortal.Services.Security;

public class PermissionService : BaseService, IPermissionService
{
    private readonly ICurrentUser _user;
    private readonly IRolePermissionProvider _provider;
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IAuthorizationService authorizationService, ICurrentUser user,
        IRolePermissionProvider provider, IPermissionRepository permissionRepository) : base(authorizationService)
    {
        _user = user;
        _provider = provider;
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        if (_user.UserId != userId) return false;
        var roles = await _user.GetRolesAsync(ct);
        var perms = await _provider.GetPermissionsForRolesAsync(roles, ct);
        return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var perms = await _permissionRepository.GetListAsync(cancellationToken: cancellationToken);

        return perms.Select(MapPermissionToDto).ToList();
    }

    public PermissionDto MapPermissionToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            FriendlyName = permission.FriendlyName,
            Area = permission.Area
        };
    }
}