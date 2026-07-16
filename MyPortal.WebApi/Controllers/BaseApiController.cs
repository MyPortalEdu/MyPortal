using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyPortal.Auth.Policies;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.WebApi.Models.Listing;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;


// Two route templates: the versioned one is canonical and shown in the OpenAPI
// doc; the unversioned one is a compatibility alias for existing callers (SPA,
// iOS) — Program.cs sets AssumeDefaultVersionWhenUnspecified so requests to
// /api/me resolve to v1 via the default-version rule. [ApiVersion("1.0")]
// cascades to every derived controller; bump or add additional [ApiVersion]
// attributes there when a specific endpoint ships a v2.
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = ScopePolicy.PolicyName)]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private readonly ProblemDetailsFactory _problemFactory;

    protected readonly ILogger Logger;

    public BaseApiController(ProblemDetailsFactory problemFactory, ILogger<BaseApiController> logger)
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
                "DuplicateUserName" or "InvalidUserName" => nameof(UserUpsertRequest.Username),
                "DuplicateEmail" or "InvalidEmail" => nameof(UserUpsertRequest.Email),
                "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or
                    "PasswordRequiresDigit" or "PasswordRequiresLower" or
                    "PasswordRequiresUpper" or "PasswordMismatch" => nameof(UserUpsertRequest.Password),
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

    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    protected ListingOptions GetListingOptions(int page, int pageSize, FilterOptions? filter, SortOptions? sort)
    {
        var options = new ListingOptions();

        // Clamp untrusted query inputs so a caller can't request `pageSize=int.MaxValue` and
        // pull the full table down in one round-trip.
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0
            ? DefaultPageSize
            : Math.Min(pageSize, MaxPageSize);

        options.PageOptions = PageOptions.Create(safePage, safePageSize);

        if (sort?.Criteria is { Length: > 0 })
        {
            options.SortOptions = sort;
        }

        if (filter?.Groups is { Length: > 0 } &&
            filter.Groups.Any(g => g.Criteria is { Length: > 0 }))
        {
            options.FilterOptions = filter;
        }

        return options;
    }
}