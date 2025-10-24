using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyPortal.Auth.Policies;
using MyPortal.Contracts.Models.System.Users;

namespace MyPortal.WebApi.Controllers;


[ApiController]
[Authorize(Policy = ScopePolicy.PolicyName)]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken] // <-- Clients may be using bearer auth
public abstract class BaseApiController<TSelf> : ControllerBase
{
    private readonly ProblemDetailsFactory _problemFactory;
    
    protected readonly ILogger<TSelf> Logger;

    public BaseApiController(ProblemDetailsFactory problemFactory, ILogger<TSelf> logger)
    {
        _problemFactory = problemFactory;
        Logger = logger;
    }

    protected CancellationToken CancellationToken => HttpContext.RequestAborted;

    protected IActionResult Problem(int statusCode, string? title = null, string? detail = null, string? type = null,
        string? instance = null, IDictionary<string, object?>? extensions = null)
    {
        var problem = _problemFactory.CreateProblemDetails(HttpContext, statusCode, title, type, detail, instance);
        
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        if (extensions is not null)
        {
            foreach (var extension in extensions)
            {
                problem.Extensions[extension.Key] = extension.Value;
            }
        }
        
        return new ObjectResult(problem) { StatusCode = statusCode };
    }
    
    protected IActionResult ValidationProblem(ModelStateDictionary modelState,
        int statusCode = StatusCodes.Status400BadRequest,
        string? title = "Validation failed.",
        string? detail = null)
    {
        var vpd = _problemFactory.CreateValidationProblemDetails(
            HttpContext,
            modelState,
            statusCode: statusCode,
            title: title,
            detail: detail);

        vpd.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(vpd) { StatusCode = statusCode };
    }

    protected IActionResult IdentityResultProblem(
        IdentityResult result,
        int statusCodeIfNotConflict = StatusCodes.Status400BadRequest)
    {
        var isConflict = result.Errors.Any(e =>
            string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase));

        var ms = new ModelStateDictionary();

        foreach (var e in result.Errors)
        {
            var key = e.Code switch
            {
                "DuplicateUserName" or "InvalidUserName" => nameof(UserUpsertDto.Username),
                "DuplicateEmail" or "InvalidEmail" => nameof(UserUpsertDto.Email),
                "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or
                    "PasswordRequiresDigit" or "PasswordRequiresLower" or
                    "PasswordRequiresUpper" or "PasswordMismatch" => nameof(UserUpsertDto.Password),
                _ => string.Empty // model-level error
            };

            ms.AddModelError(key, e.Description);
        }
        
        var extensions = new Dictionary<string, object?>
        {
            ["identityErrors"] = result.Errors.Select(e => new { e.Code, e.Description })
        };

        if (isConflict)
            return Problem(StatusCodes.Status409Conflict, "Conflict",
                "One or more unique constraints were violated.", extensions: extensions);

        return ValidationProblem(ms, statusCode: statusCodeIfNotConflict, title: "Validation failed.");
    }

    protected IActionResult NotFoundProblem(string detail, string? type = null) =>
        Problem(StatusCodes.Status404NotFound, "Not Found", detail, type);

    protected IActionResult BadRequestProblem(string detail, string? type = null) =>
        Problem(StatusCodes.Status400BadRequest, "Bad Request", detail, type);

    protected IActionResult UnauthorizedProblem(string detail, string? type = null) =>
        Problem(StatusCodes.Status401Unauthorized, "Unauthorized", detail, type);
}