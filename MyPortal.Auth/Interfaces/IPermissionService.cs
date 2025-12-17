using MyPortal.Contracts.Models.System.Permissions;

namespace MyPortal.Auth.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default);

    Task<IList<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken);
}