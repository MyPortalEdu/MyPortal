using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface IAuthorizationService
{
    Guid? GetCurrentUserId();
    Guid? GetCurrentUserPersonId();
    string? GetCurrentUserIpAddress();
    UserType GetCurrentUserType();
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken);
    Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task RequirePermissionAsync(string permission, CancellationToken ct = default);
    void RequireUserType(UserType userType);
}