using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Auth.Extensions;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// OAuth 2.0 / OpenID Connect endpoints (authorize, token, userinfo, end session).
/// These are passthrough handlers that delegate the heavy lifting to OpenIddict;
/// the OpenIddict server settings are wired up in <c>Program.cs</c>. Used by the
/// Scalar UI's "Authorize" flow and by future native clients (e.g. iOS app)
/// running the Authorization Code + PKCE flow.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("connect")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>OAuth 2.0 authorization endpoint (GET).</summary>
    /// <remarks>
    /// Entry point for the Authorization Code + PKCE flow. If the caller isn't
    /// signed in, redirects them to the local Razor login page; once authenticated
    /// the request is re-issued and OpenIddict mints an authorization code that's
    /// returned to the registered redirect URI. Anonymous (unsigned) callers get
    /// challenged here, not 401.
    /// </remarks>
    [HttpGet("authorize")]
    public async Task<IActionResult> GetAuthorize()
    {
        return User.Identity?.IsAuthenticated == true
            ? await PostAuthorize()
            : Challenge(IdentityConstants.ApplicationScheme);
    }

    /// <summary>OAuth 2.0 authorization endpoint (POST).</summary>
    /// <remarks>
    /// Same flow as the GET variant; called by user agents that re-submit the
    /// authorize request after the consent screen. Returns the OpenIddict-built
    /// SignIn result that produces the redirect with the authorization code.
    /// Antiforgery is intentionally bypassed — this is an OAuth endpoint, not a
    /// browser form post.
    /// </remarks>
    [HttpPost("authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PostAuthorize()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OIDC request missing.");

        var user = await _userManager.GetUserAsync(User)
                   ?? throw new InvalidOperationException("User not found.");

        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        if (principal.GetClaim(OpenIddictConstants.Claims.Subject) is null)
            principal.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());

        principal.SetScopes(new[]
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.OfflineAccess,
            "api"
        }.Intersect(request.GetScopes()));

        principal.SetDestinations();

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>OAuth 2.0 token endpoint.</summary>
    /// <remarks>
    /// Exchanges an authorization code (with PKCE verifier) for an access token,
    /// or trades a refresh token for a new access token. <c>IsEnabled</c> is
    /// re-checked on every refresh so disabling a user invalidates their device
    /// within one access-token lifetime (60 min). Only authorization_code and
    /// refresh_token grant types are supported; anything else is rejected as
    /// <c>unsupported_grant_type</c>.
    /// </remarks>
    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        const string oidcScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

        // AUTHORIZATION CODE or REFRESH TOKEN:
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(oidcScheme);
            if (!result.Succeeded || result.Principal is null)
                return Forbid(oidcScheme);

            var subject = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);

            if (string.IsNullOrEmpty(subject))
                return Forbid(oidcScheme);

            var user = await _userManager.FindByIdAsync(subject);

            if (user is null || !user.IsEnabled)
                return Forbid(oidcScheme);

            return SignIn(result.Principal, oidcScheme);
        }

        return BadRequest(new { error = OpenIddictConstants.Errors.UnsupportedGrantType });
    }

    /// <summary>OpenID Connect userinfo endpoint.</summary>
    /// <remarks>
    /// Bearer-only — requires an access token issued via the OAuth flow. Returns
    /// a minimal subject + user-type payload; expand here if more standard claims
    /// (name, email, etc.) are needed by clients.
    /// </remarks>
    [Authorize(AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("userinfo")]
    public IActionResult UserInfo([FromServices] ICurrentUser me)
    {
        if (!User.HasScope(OpenIddictConstants.Scopes.OpenId))
        {
            return Forbid();
        }

        return Ok(new { userId = me.UserId, userType = me.UserType.ToString() });
    }

    /// <summary>OpenID Connect end-session endpoint (GET).</summary>
    /// <remarks>
    /// Signs the user out of the local cookie session and invokes OpenIddict's
    /// end-session handling, which redirects to the registered post-logout URI
    /// (or <c>/</c> if none was supplied).
    /// </remarks>
    [HttpGet("endsession")]
    public Task<IActionResult> GetEndSession() => HandleEndSessionAsync();

    /// <summary>OpenID Connect end-session endpoint (POST).</summary>
    /// <remarks>Same behaviour as the GET variant, for clients that POST to logout.</remarks>
    [HttpPost("endsession")]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> PostEndSession() => HandleEndSessionAsync();

    private async Task<IActionResult> HandleEndSessionAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OIDC request missing.");

        // Sign out the local cookie session
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Hand control back to OpenIddict to complete end-session + redirect
        var props = new AuthenticationProperties
        {
            RedirectUri = request.PostLogoutRedirectUri ?? "/"
        };

        return SignOut(props, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}