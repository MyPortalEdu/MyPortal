using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyPortal.Auth.Models;

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

    public void OnGet(string? returnUrl = "/") => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
    {
        if (!ModelState.IsValid) { ReturnUrl = returnUrl; return Page(); }

        var user = await _users.FindByNameAsync(Input.Username);
        if (user is null)
        { ModelState.AddModelError(string.Empty, "Invalid credentials."); ReturnUrl = returnUrl; return Page(); }

        var result = await _signIn.PasswordSignInAsync(user, Input.Password, Input.RememberMe, true);
        if (!result.Succeeded)
        { ModelState.AddModelError(string.Empty, "Invalid credentials."); ReturnUrl = returnUrl; return Page(); }

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}