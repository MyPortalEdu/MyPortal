using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Requirements;

namespace MyPortal.Auth.Providers;

public sealed class ApplicationPolicyProvider(IServiceProvider sp) : IAuthorizationPolicyProvider
{
    private const string PermPrefix = "perm:";
    private const string UtPrefix   = "ut:";

    private readonly DefaultAuthorizationPolicyProvider _fallback = new(
        sp.GetRequiredService<IOptions<AuthorizationOptions>>());
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            return _fallback.GetPolicyAsync(policyName);

        if (_cache.TryGetValue(policyName, out var cached))
            return Task.FromResult<AuthorizationPolicy?>(cached);

        if (policyName.StartsWith(PermPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var payload = policyName[PermPrefix.Length..];
            var parts = payload.Split(':', 2);

            // Malformed policy name: fall through to the default provider (which returns null
            // for unknown policies), so misconfigurations surface as "policy not found" rather
            // than silently degrading to RequireAny with whatever permissions follow.
            if (!Enum.TryParse(parts[0], ignoreCase: true, out PermissionMode mode))
            {
                return _fallback.GetPolicyAsync(policyName);
            }

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
