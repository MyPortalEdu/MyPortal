using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyPortal.Auth.Models;

namespace MyPortal.WebApi.Areas.Account.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    public LogoutModel(SignInManager<ApplicationUser> signIn) => _signIn = signIn;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
    {
        await _signIn.SignOutAsync();
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}