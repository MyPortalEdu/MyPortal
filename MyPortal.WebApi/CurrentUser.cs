using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;

namespace MyPortal.WebApi;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRoleAccessor _roles;
    private readonly IMemoryCache _cache;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    
    public CurrentUser(IHttpContextAccessor httpContextAccessor, IRoleAccessor roles, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _roles = roles;
        _cache = cache;
    }
    
    public Guid? UserId
    {
        get
        {
            return Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
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
            return await Task.FromResult<IReadOnlyCollection<Guid>>([]);
        
        var key = $"roles:{UserId}";

        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await _roles.GetRolesForUserAsync(UserId.Value, ct);
        })! ?? [];
    }
}