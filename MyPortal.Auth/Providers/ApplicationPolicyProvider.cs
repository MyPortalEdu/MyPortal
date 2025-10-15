using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Requirements;

namespace MyPortal.Auth.Providers;

public sealed class ApplicationPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermPrefix = "perm:";
    private const string UtPrefix   = "ut:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    public ApplicationPolicyProvider(IServiceProvider sp)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(
            sp.GetRequiredService<IOptions<AuthorizationOptions>>());
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            return _fallback.GetPolicyAsync(policyName);

        // cache any custom policy by name
        if (_cache.TryGetValue(policyName, out var cached))
            return Task.FromResult<AuthorizationPolicy?>(cached);

        if (policyName.StartsWith(PermPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var payload = policyName[PermPrefix.Length..];
            var parts = payload.Split(':', 2);

            var mode = Enum.TryParse(parts[0], ignoreCase: true, out PermissionMode req)
                ? req : PermissionMode.RequireAny;

            var perms = (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                ? parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(mode, perms))
                .Build();

            _cache.TryAdd(policyName, policy);
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName.StartsWith(UtPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var allowed = policyName[UtPrefix.Length..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new UserTypeRequirement(allowed))
                .Build();

            _cache.TryAdd(policyName, policy);
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();
}
