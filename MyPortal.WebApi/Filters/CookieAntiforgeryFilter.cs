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
    private readonly ILogger<CookieAntiforgeryFilter> _logger;

    public CookieAntiforgeryFilter(IAntiforgery antiforgery, ILogger<CookieAntiforgeryFilter> logger)
    {
        _antiforgery = antiforgery;
        _logger = logger;
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
        catch (AntiforgeryValidationException ex)
        {
            // ASP.NET's AntiforgeryValidationException message is specific (e.g.
            // "The provided antiforgery token was meant for user 'A', but the
            // current user is 'B'.") and the most useful signal for diagnosing
            // why CSRF validation is rejecting an otherwise-well-formed request.
            // Log it at Warning so it surfaces in dev consoles without spamming
            // info-level traces.
            _logger.LogWarning(ex,
                "Antiforgery validation failed for {Method} {Path} (user: {User}, hasStorageCookie: {HasStorage}, hasHeader: {HasHeader})",
                method,
                context.HttpContext.Request.Path,
                context.HttpContext.User?.Identity?.Name ?? "(anonymous)",
                context.HttpContext.Request.Cookies.Keys.Any(c => c.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal)),
                context.HttpContext.Request.Headers.ContainsKey("X-XSRF-TOKEN"));

            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
