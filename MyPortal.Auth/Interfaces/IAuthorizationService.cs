using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface IAuthorizationService
{
    Guid? GetCurrentUserId();

    /// <summary>The Person the current user is linked to, or null for users with no person
    /// identity. Needed to resolve self / line-management relationships.</summary>
    Guid? GetCurrentUserPersonId();

    string? GetCurrentUserIpAddress();
    UserType GetCurrentUserType();
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken);

    /// <summary>The current user's effective permission set (case-insensitive). Resolve once and
    /// test membership in memory when evaluating many permissions (e.g. a capability map).</summary>
    Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    Task RequirePermissionAsync(string permission, CancellationToken ct = default);
    void RequireUserType(UserType userType);
}