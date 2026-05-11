using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MyPortal.Auth.Attributes;
using MyPortal.WebApi.Infrastructure.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyPortal.WebApi.Swagger;

/// <summary>
/// Adds the standard error response shapes (400 / 401 / 403) to every operation
/// that matches the relevant attribute pattern, so we don't have to repeat
/// <c>[ProducesResponseType]</c> for them on every endpoint. The success-case
/// shape is still declared per-endpoint via <c>[ProducesResponseType]</c>.
/// </summary>
public sealed class ResponseTypesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerAttrs = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [];
        var methodAttrs = context.MethodInfo.GetCustomAttributes(true);

        var hasAuthorize =
            controllerAttrs.OfType<AuthorizeAttribute>().Any()
            || methodAttrs.OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymous =
            controllerAttrs.OfType<AllowAnonymousAttribute>().Any()
            || methodAttrs.OfType<AllowAnonymousAttribute>().Any();

        var authorized = hasAuthorize && !hasAllowAnonymous;

        var hasPermissionGate =
            methodAttrs.OfType<PermissionAttribute>().Any()
            || methodAttrs.OfType<UserTypeAttribute>().Any();

        var hasValidateModel = methodAttrs.OfType<ValidateModelAttribute>().Any();

        operation.Responses ??= new OpenApiResponses();

        if (hasValidateModel)
        {
            operation.Responses["400"] = ProblemResponse(
                context,
                typeof(ValidationProblemDetails),
                "Validation failed — one or more fields are missing or invalid.");
        }

        if (authorized)
        {
            operation.Responses["401"] = ProblemResponse(
                context,
                typeof(ProblemDetails),
                "The caller is not authenticated.");
        }

        if (authorized && hasPermissionGate)
        {
            operation.Responses["403"] = ProblemResponse(
                context,
                typeof(ProblemDetails),
                "The caller is authenticated but lacks the required permission or user type.");
        }
    }

    private static OpenApiResponse ProblemResponse(OperationFilterContext context, Type type, string description)
    {
        var schema = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);

        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new() { Schema = schema }
            }
        };
    }
}
