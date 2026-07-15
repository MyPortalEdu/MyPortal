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

            // If a cookie identity is present anywhere in the principal, allow.
            // Otherwise (bearer-only) require scope=api.
            //
            // We check Identities, not Identity. When AddAuthenticationSchemes
            // runs both schemes and they both succeed, PolicyEvaluator merges
            // their principals via SecurityHelper.MergeUserPrincipal — which
            // prepends new identities, so the bearer one ends up primary and
            // `User.Identity` no longer points at the cookie identity even
            // when one exists. This was responsible for Scalar's API calls
            // 403ing whenever the caller was also logged in via cookie.
            policy.RequireAssertion(ctx =>
            {
                var hasCookieIdentity = ctx.User.Identities.Any(i =>
                    i.IsAuthenticated &&
                    string.Equals(i.AuthenticationType, IdentityConstants.ApplicationScheme,
                        StringComparison.OrdinalIgnoreCase));

                if (hasCookieIdentity) return true;

                // OpenIddict can serialise scopes either as one space-separated
                // claim or as multiple individual claims; SelectMany + Split
                // handles both shapes.
                var scopes = ctx.User.Claims
                    .Where(c => c.Type == "scope")
                    .SelectMany(c =>
                        c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                return scopes.Contains("api", StringComparer.Ordinal);
            });
        }
    }
}