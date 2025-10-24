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

    [HttpGet("authorize")]
    public async Task<IActionResult> GetAuthorize()
    {
        return User.Identity?.IsAuthenticated == true
            ? await PostAuthorize()
            : Challenge(IdentityConstants.ApplicationScheme);
    }

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

            // optionally re-check user.Enabled here by loading the user id from NameIdentifier
            // and forbidding if disabled, depending on token invalidation strategy.

            return SignIn(result.Principal, oidcScheme);
        }

        return BadRequest(new { error = OpenIddictConstants.Errors.UnsupportedGrantType });
    }

    [Authorize(AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("userinfo")]
    public IActionResult UserInfo([FromServices] ICurrentUser me)
    {
        if (!User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains(OpenIddictConstants.Scopes.OpenId)))
            return Forbid();

        return Ok(new { userId = me.UserId, userType = me.UserType.ToString() });
    }

    [HttpGet("endsession")]
    public Task<IActionResult> GetEndSession() => HandleEndSessionAsync();

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