using System.Security.Claims;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;

namespace MyPortal.WebApi;

public class CurrentUser(IHttpContextAccessor httpContextAccessor, IRoleAccessor roles)
    : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public Guid? UserId
    {
        get
        {
            return Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : null;
        }
    }

    public Guid? PersonId
    {
        get
        {
            return Guid.TryParse(Principal?.FindFirstValue(Auth.Constants.ClaimTypes.PersonId), out var personId)
                ? personId
                : null;
        }
    }

    public UserType UserType
    {
        get
        {
            var raw = Principal?.FindFirstValue(Auth.Constants.ClaimTypes.UserType);
            return Enum.TryParse<UserType>(raw, true, out var t) ? t : UserType.Unknown;
        }
    }

    public async Task<IReadOnlyCollection<Guid>> GetRolesAsync(CancellationToken ct = default)
    {
        if (UserId is null)
            return [];

        return await roles.GetRolesForUserAsync(UserId.Value, ct);
    }
}
