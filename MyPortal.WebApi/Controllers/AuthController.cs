using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyPortal.Auth;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
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
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OIDC request missing.");
        
        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username!)
                       ?? await _userManager.FindByEmailAsync(request.Username!);

            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var pwd = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: false);
            if (!pwd.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var id = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
            id.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(),
                OpenIddictConstants.Destinations.AccessToken);
            id.AddClaim("ut", user.UserType.ToString(), OpenIddictConstants.Destinations.AccessToken);
            if (!string.IsNullOrEmpty(user.SecurityStamp))
                id.AddClaim("usrver", user.SecurityStamp, OpenIddictConstants.Destinations.AccessToken);

            var principal = new ClaimsPrincipal(id);
            
            principal.SetScopes(request.GetScopes());

            principal.SetDestinations(claim => claim.Type switch
            {
                OpenIddictConstants.Claims.Subject => new[] { OpenIddictConstants.Destinations.AccessToken },
                "ut" => new[] { OpenIddictConstants.Destinations.AccessToken },
                "usrver" => new[] { OpenIddictConstants.Destinations.AccessToken },
                _ => Array.Empty<string>()
            });

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // AUTHORIZATION CODE or REFRESH TOKEN:
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var principal = result.Principal;

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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