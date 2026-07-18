using System.Security.Authentication;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;

namespace MyPortal.Services.Security;

public class AuthorizationService(ICurrentUser user, IPermissionService perms) : IAuthorizationService
{
    public Guid? GetCurrentUserId()
    {
        return user.UserId;
    }

    public Guid? GetCurrentUserPersonId()
    {
        return user.PersonId;
    }

    public string? GetCurrentUserIpAddress()
    {
        return user.IpAddress;
    }

    public UserType GetCurrentUserType()
    {
        return user.UserType;
    }

    public Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken)
    {
        return perms.HasPermissionAsync(permission, cancellationToken);
    }

    public Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return perms.GetCurrentUserPermissionsAsync(cancellationToken);
    }

    public async Task RequirePermissionAsync(string permission, CancellationToken ct)
    {
        if (user.UserId is null) throw new AuthenticationException("Not authenticated.");
        if (!await perms.HasPermissionAsync(permission, ct))
            throw new ForbiddenException($"You do not have permission to perform this action.");
    }

    public void RequireUserType(UserType userType)
    {
        if (user.UserType != userType)
        {
            throw new ForbiddenException($"You must be user type '{userType}' to perform this action.");
        }
    }
}