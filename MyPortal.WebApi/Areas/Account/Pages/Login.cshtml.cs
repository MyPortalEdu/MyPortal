using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyPortal.Auth.Models;
using MyPortal.WebApi.Infrastructure.Extensions;

namespace MyPortal.WebApi.Areas.Account.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    { _signIn = signIn; _users = users; }

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required] public string Username { get; set; } = "";
        [Required] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public IActionResult OnGet(string? returnUrl = "/")
    {
        ReturnUrl = returnUrl;

        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
    {
        if (!ModelState.IsValid) { ReturnUrl = returnUrl; return Page(); }

        var user = await _users.FindByNameAsync(Input.Username);
        if (user is null)
        {
            // Same wording as a wrong password so we don't reveal whether the username exists.
            ModelState.AddModelError(string.Empty, "Username or password incorrect.");
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _signIn.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            // PasswordSignInAsync runs CanSignInAsync (which rejects !IsEnabled) before the
            // password check, so a disabled account surfaces as IsNotAllowed regardless of password.
            var message = result.IsNotAllowed
                ? "Your account is disabled. Please speak to your school administrator."
                : result.IsLockedOut
                    ? "Your account is locked after too many failed attempts. Please try again later."
                    : "Username or password incorrect.";
            ModelState.AddModelError(string.Empty, message);
            ReturnUrl = returnUrl;
            return Page();
        }

        // Refresh the antiforgery cookie BEFORE the redirect leaves this server.
        // In dev, ng serve doesn't proxy SPA routes (e.g. /staff) back to the API,
        // so the global middleware that normally re-mints on shell loads never
        // fires on the post-login navigation — the SPA would keep the anonymous-
        // bound XSRF-TOKEN from earlier and the first POST would 403 with
        // "antiforgery token was meant for a different claims-based user".
        //
        // PasswordSignInAsync only updates the response auth cookie, not the
        // current request's HttpContext.User, so we promote the principal here
        // first; otherwise IAntiforgery would still bind the new token to the
        // anonymous identity.
        HttpContext.User = await _signIn.CreateUserPrincipalAsync(user);
        HttpContext.RefreshXsrfCookie();

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}
