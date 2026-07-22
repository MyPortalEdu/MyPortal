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

            policy.AddAuthenticationSchemes(
                IdentityConstants.ApplicationScheme,
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            
            policy.RequireAssertion(ctx =>
            {
                var hasCookieIdentity = ctx.User.Identities.Any(i =>
                    i.IsAuthenticated &&
                    string.Equals(i.AuthenticationType, IdentityConstants.ApplicationScheme,
                        StringComparison.OrdinalIgnoreCase));

                if (hasCookieIdentity) return true;
                
                var scopes = ctx.User.Claims
                    .Where(c => c.Type == "scope")
                    .SelectMany(c =>
                        c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                return scopes.Contains("api", StringComparer.Ordinal);
            });
        }
    }
}