using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Permissions;

namespace MyPortal.Auth.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);

    Task<bool> HasAnyPermissionsAsync(string[] permissions, CancellationToken ct = default);

    Task<bool> HasAllPermissionsAsync(string[] permissions, CancellationToken ct = default);

    // userType filters the catalogue to one portal's permissions (the role-editor shows only the
    // permissions matching the role's audience). Null returns every permission.
    Task<IList<PermissionResponse>> GetAllPermissionsAsync(UserType? userType, CancellationToken cancellationToken);
}