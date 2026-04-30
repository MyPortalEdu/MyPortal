using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MyPortal.WebApi.Filters;

/// <summary>
/// Validates the antiforgery token on unsafe HTTP methods, but only when the
/// request carries an ASP.NET Core auth cookie. Bearer-authenticated requests
/// (e.g., the SPA after token exchange, mobile clients) are skipped — they
/// are not vulnerable to CSRF since attacker pages cannot mint or replay
/// bearer tokens cross-site. Honors [IgnoreAntiforgeryToken] for opt-outs
/// (OAuth/OIDC endpoints).
/// </summary>
public sealed class CookieAntiforgeryFilter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    public CookieAntiforgeryFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
        {
            return;
        }

        if (context.ActionDescriptor.EndpointMetadata.OfType<IgnoreAntiforgeryTokenAttribute>().Any())
        {
            return;
        }

        var hasAuthCookie = context.HttpContext.Request.Cookies.Keys
            .Any(c => c.StartsWith(".AspNetCore.", StringComparison.Ordinal));

        if (!hasAuthCookie)
        {
            return;
        }

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
