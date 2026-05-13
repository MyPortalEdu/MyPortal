using Microsoft.AspNetCore.Antiforgery;

namespace MyPortal.WebApi.Infrastructure.Extensions;

public static class AntiforgeryHttpContextExtensions
{
    /// <summary>
    /// (Re)mint the antiforgery tokens for the current request and write the
    /// SPA-readable XSRF-TOKEN cookie. The request-token's claim-uid is bound
    /// to <see cref="HttpContext.User"/> as it stands when this is called —
    /// callers that have just signed a user in must promote
    /// <see cref="HttpContext.User"/> to the new principal first, otherwise
    /// the token stays bound to the previous (anonymous) identity and the
    /// next state-changing request will fail antiforgery validation.
    ///
    /// Idempotent and cheap when a valid storage cookie already exists:
    /// IAntiforgery reuses the SecurityToken and only regenerates the
    /// per-user request token.
    /// </summary>
    public static void RefreshXsrfCookie(this HttpContext ctx)
    {
        var anti = ctx.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = anti.GetAndStoreTokens(ctx);

        ctx.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,            // Angular needs to read this to populate X-XSRF-TOKEN
            Secure = true,                // Safe in prod: UseHttpsRedirection() ensures HTTPS responses
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
}
