using MyPortal.Contracts.Models.System.Permissions;

namespace MyPortal.Auth.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);
    
    Task<bool> HasAnyPermissionsAsync(string[] permissions, CancellationToken ct = default);
    
    Task<bool> HasAllPermissionsAsync(string[] permissions, CancellationToken ct = default);

    /// <summary>
    /// The current user's effective permission set (case-insensitive). Empty when unauthenticated
    /// or disabled. Resolve once and test membership in memory when evaluating many permissions
    /// (e.g. building a capability map) rather than calling <see cref="HasPermissionAsync"/> N times.
    /// </summary>
    Task<IReadOnlySet<string>> GetCurrentUserPermissionsAsync(CancellationToken ct = default);

    Task<IList<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken);
}