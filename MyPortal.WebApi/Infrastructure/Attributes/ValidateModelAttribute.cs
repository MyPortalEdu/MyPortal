using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyPortal.Services.Interfaces;

namespace MyPortal.WebApi.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateModelAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validationService = context.HttpContext.RequestServices.GetRequiredService<IValidationService>();
        var problemFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var failures = await validationService.TryValidateAsync(arg);
            if (failures.Count > 0)
            {
                var ms = new ModelStateDictionary();
                foreach (var f in failures)
                    ms.AddModelError(f.PropertyName, f.ErrorMessage);

                var problem = problemFactory.CreateValidationProblemDetails(
                    context.HttpContext,
                    ms,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation failed."
                );

                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                context.Result = new ObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

                return; // short-circuit the action
            }
        }

        await next(); // continue pipeline
    }
}