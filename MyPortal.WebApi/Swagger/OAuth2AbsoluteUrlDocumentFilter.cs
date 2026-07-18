using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyPortal.WebApi.Swagger;

/// <summary>
/// Rewrites the OAuth2 authorisation/token URLs in the served OpenAPI document
/// to absolute URLs based on the current request's host. Scalar's OAuth flow
/// requires absolute URLs; serving relative ones causes "Failed to authorize
/// oauth2 flow" because the client-side URL constructor rejects them.
/// </summary>
public sealed class OAuth2AbsoluteUrlDocumentFilter(IHttpContextAccessor httpAccessor) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var ctx = httpAccessor.HttpContext;
        if (ctx is null) return;

        if (swaggerDoc.Components?.SecuritySchemes is null) return;
        if (!swaggerDoc.Components.SecuritySchemes.TryGetValue("OAuth2", out var scheme)) return;
        if (scheme.Flows?.AuthorizationCode is not { } flow) return;

        var origin = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        flow.AuthorizationUrl = new Uri($"{origin}/connect/authorize");
        flow.TokenUrl = new Uri($"{origin}/connect/token");
    }
}
