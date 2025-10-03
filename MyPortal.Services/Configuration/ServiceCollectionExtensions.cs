using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Auth.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Services.Security;
using MyPortal.Services.Services;
using MyPortal.Services.Validation;

namespace MyPortal.Services.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPortalServices(this IServiceCollection services)
    {
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}