using System.Security.Authentication;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;

namespace MyPortal.Services.Security;

public class AuthorizationService : IAuthorizationService
{
    private readonly ICurrentUser _user;
    private readonly IPermissionService _perms;

    public AuthorizationService(ICurrentUser user, IPermissionService perms)
        => (_user, _perms) = (user, perms);

    public Guid? GetCurrentUserId()
    {
        return _user.UserId;
    }

    public string? GetCurrentUserIpAddress()
    {
        return _user.IpAddress;
    }

    public UserType GetCurrentUserType()
    {
        return _user.UserType;
    }

    public Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken)
    {
        return _perms.HasPermissionAsync(permission, cancellationToken);
    }

    public async Task RequirePermissionAsync(string permission, CancellationToken ct)
    {
        if (_user.UserId is null) throw new AuthenticationException("Not authenticated.");
        if (!await _perms.HasPermissionAsync(permission, ct))
            throw new ForbiddenException($"You do not have permission to perform this action.");
    }

    public void RequireUserType(UserType userType)
    {
        if (_user.UserType != userType)
        {
            throw new ForbiddenException($"You must be user type '{userType}' to perform this action.");
        }
    }
}