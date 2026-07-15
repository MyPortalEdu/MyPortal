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

/// <summary>OAuth 2.0 / OpenID Connect endpoints.</summary>
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
    /// <remarks>Challenges anonymous callers and resumes the authorize flow after sign-in.</remarks>
    [HttpGet("authorize")]
    public async Task<IActionResult> GetAuthorize()
    {
        return User.Identity?.IsAuthenticated == true
            ? await PostAuthorize()
            : Challenge(IdentityConstants.ApplicationScheme);
    }

    /// <summary>OAuth 2.0 authorization endpoint (POST).</summary>
    /// <remarks>Same flow as GET. Antiforgery is intentionally bypassed.</remarks>
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
    /// <remarks>Supports authorization code and refresh token grants.</remarks>
    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        const string oidcScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

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
    /// <remarks>Requires a bearer token and returns the current subject and user type.</remarks>
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
    /// <remarks>Signs out the local session and redirects to the post-logout URI.</remarks>
    [HttpGet("endsession")]
    public Task<IActionResult> GetEndSession() => HandleEndSessionAsync();

    /// <summary>OpenID Connect end-session endpoint (POST).</summary>
    /// <remarks>Same behaviour as the GET variant.</remarks>
    [HttpPost("endsession")]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> PostEndSession() => HandleEndSessionAsync();

    private async Task<IActionResult> HandleEndSessionAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OIDC request missing.");

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Hand control back to OpenIddict to complete end-session + redirect
        var props = new AuthenticationProperties
        {
            RedirectUri = request.PostLogoutRedirectUri ?? "/"
        };

        return SignOut(props, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
