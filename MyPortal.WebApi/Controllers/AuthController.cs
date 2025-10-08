using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace MyPortal.WebApi.Controllers;

[ApiController]
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

    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        const string oidcScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username);

            if (user == null || !await _signInManager.CanSignInAsync(user))
            {
                return Forbid(oidcScheme);
            }

            var pwd = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!pwd.Succeeded || pwd.IsLockedOut)
            {
                return Forbid(oidcScheme);
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.OfflineAccess,
                "api"
            }.Intersect(request.GetScopes()));

            principal.SetResources("api");

            principal.SetDestinations(claim =>
            {
                return claim.Type switch
                {
                    ClaimTypes.NameIdentifier => new[] { OpenIddictConstants.Destinations.AccessToken },
                    Auth.Constants.ClaimTypes.UserType => new[] { OpenIddictConstants.Destinations.AccessToken },
                };
            });

            return SignIn(principal, oidcScheme);
        }

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
    
    [Authorize]
    [HttpGet("userinfo")]
    public IActionResult UserInfo([FromServices] ICurrentUser me)
    {
        return Ok(new { userId = me.UserId, userType = me.UserType.ToString() });
    }
}