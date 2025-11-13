using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface IAuthorizationService
{
    Guid? GetCurrentUserId();
    UserType GetCurrentUserType();
    Task RequirePermissionAsync(string permission, CancellationToken ct = default);
    void RequireUserType(UserType userType);
}