using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using MyPortal.Services.Interfaces;

namespace MyPortal.WebApi.Infrastructure.Attributes;

/// <summary>
/// Action filter that runs the FluentValidation-backed IValidationService
/// across each non-null action argument and short-circuits with an
/// <c>application/problem+json</c> 400 (RFC 7807 ValidationProblemDetails)
/// when any rule fails. Output shape and content-type mirror what
/// <see cref="MyPortal.WebApi.Infrastructure.Middleware.ExceptionMiddleware"/>
/// emits for thrown <c>FluentValidation.ValidationException</c>s, so SPA
/// clients can parse both the same way.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateModelAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validationService = context.HttpContext.RequestServices.GetRequiredService<IValidationService>();
        var problemFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        // Aggregate failures across every action argument before bailing —
        // single-DTO endpoints behave as before, but endpoints with multiple
        // bound DTOs (e.g. a mix of route/body, or two body DTOs) now surface
        // every broken field in one response instead of one per request retry.
        var ms = new ModelStateDictionary();
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var failures = await validationService.TryValidateAsync(arg);
            foreach (var f in failures)
                ms.AddModelError(f.PropertyName, f.ErrorMessage);
        }

        if (ms.IsValid)
        {
            await next();
            return;
        }

        var problem = problemFactory.CreateValidationProblemDetails(
            context.HttpContext,
            ms,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation failed.");

        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        // Pin ContentTypes to application/problem+json so the negotiated
        // response header matches ExceptionMiddleware exactly, regardless of
        // the caller's Accept header. Without this, MVC's content negotiation
        // can downgrade the response to plain application/json when the
        // client sent Accept: application/json — same JSON body, but a
        // different header, which means SPA clients have to handle both.
        var result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
        result.ContentTypes.Add(MediaTypeHeaderValue.Parse("application/problem+json"));
        context.Result = result;
    }
}
