using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyPortal.Common.Exceptions;

namespace MyPortal.WebApi.Infrastructure.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemFactory;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, ProblemDetailsFactory problemFactory)
    {
        _logger = logger;
        _problemFactory = problemFactory;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException vex)
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in vex.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);

            var problem = _problemFactory.CreateValidationProblemDetails(
                context,
                modelState,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed.");

            await WriteProblemAsync(context, problem);
        }
        catch (ForbiddenException pex)
        {
            await WriteProblemAsync(context,
                _problemFactory.CreateProblemDetails(context,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden.",
                    detail: pex.Message));
        }
        catch (AcademicYearLockedException aex)
        {
            await WriteProblemAsync(context,
                _problemFactory.CreateProblemDetails(context,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Academic year locked.",
                    detail: aex.Message));
        }
        catch (NotFoundException nex)
        {
            await WriteProblemAsync(context,
                _problemFactory.CreateProblemDetails(context,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found.",
                    detail: nex.Message));
        }
        catch (SystemEntityException sex)
        {
            await WriteProblemAsync(context,
                _problemFactory.CreateProblemDetails(context,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid operation.",
                    detail: sex.Message));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var pd = _problemFactory.CreateProblemDetails(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server error",
                detail: "An unexpected error occurred.");
            pd.Instance = context.Request.Path;
            await WriteProblemAsync(context, pd);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted) return;

        // Enrich with trace id
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}