namespace MyPortal.Auth.Interfaces;

public interface IRolePermissionProvider
{
    Task<IReadOnlyCollection<string>> GetPermissionsForRolesAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default);
}