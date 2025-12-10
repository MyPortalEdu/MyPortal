using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Auth.Interfaces;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Services.People;
using MyPortal.Services.School;
using MyPortal.Services.Security;
using MyPortal.Services.System;
using MyPortal.Services.Validation;

namespace MyPortal.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPortalServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}