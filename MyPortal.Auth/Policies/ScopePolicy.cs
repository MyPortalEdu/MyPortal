using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

namespace MyPortal.Auth.Policies
{
    public static class ScopePolicy
    {
        public const string PolicyName = "RequireApiScopeOrCookie";

        public static void ConfigurePolicy(AuthorizationPolicyBuilder policy)
        {
            policy.RequireAuthenticatedUser();

            // Accept either cookie or bearer
            policy.AddAuthenticationSchemes(
                IdentityConstants.ApplicationScheme,
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

            // If bearer -> must have scope=api. If cookie -> allowed.
            policy.RequireAssertion(ctx =>
            {
                var scheme = ctx.User.Identity?.AuthenticationType ?? string.Empty;

                if (string.Equals(scheme, IdentityConstants.ApplicationScheme, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.Equals(scheme, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var scopes = ctx.User.Claims
                    .Where(c => c.Type == "scope")
                    .SelectMany(c =>
                        c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                return scopes.Contains("api", StringComparer.Ordinal);
            });
        }
    }
}