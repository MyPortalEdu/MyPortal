using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface IAuthorizationService
{
    Guid? GetCurrentUserId();
    string? GetCurrentUserIpAddress();
    UserType GetCurrentUserType();
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken);
    Task RequirePermissionAsync(string permission, CancellationToken ct = default);
    void RequireUserType(UserType userType);
}