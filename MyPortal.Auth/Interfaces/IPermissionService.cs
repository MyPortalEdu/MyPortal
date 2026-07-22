using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Permissions;

namespace MyPortal.Auth.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);

    Task<bool> HasAnyPermissionsAsync(string[] permissions, CancellationToken ct = default);

    Task<bool> HasAllPermissionsAsync(string[] permissions, CancellationToken ct = default);
    
    Task<IReadOnlySet<string>> GetCurrentUserPermissionsAsync(CancellationToken ct = default);
    
    Task<IList<PermissionResponse>> GetAllPermissionsAsync(UserType? userType, CancellationToken cancellationToken);
}