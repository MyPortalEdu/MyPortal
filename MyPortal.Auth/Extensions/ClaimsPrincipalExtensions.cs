using OpenIddict.Abstractions;
using System.Security.Claims;

namespace MyPortal.Auth.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static ClaimsPrincipal SetDestinations(this ClaimsPrincipal principal)
    {
        principal.SetDestinations(claim =>
        {
            bool Has(string scope) => principal.HasScope(scope);

            return claim.Type switch
            {
                // OIDC Claims

                OpenIddictConstants.Claims.Subject
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                OpenIddictConstants.Claims.Name when Has(OpenIddictConstants.Permissions.Scopes.Profile)
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                OpenIddictConstants.Claims.Email when Has(OpenIddictConstants.Permissions.Scopes.Email)
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                OpenIddictConstants.Claims.Role
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                // Identity Claims

                ClaimTypes.NameIdentifier
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                // MyPortal Custom Claims

                Constants.ClaimTypes.UserType
                    => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                _ => Array.Empty<string>()
            };
        });

        return principal;
    }
}