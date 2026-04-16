using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Auth.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.Interfaces.Security;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Services.People;
using MyPortal.Services.Providers;
using MyPortal.Services.School;
using MyPortal.Services.School.Bulletins;
using MyPortal.Services.Security;
using MyPortal.Services.System;
using MyPortal.Services.Validation;

namespace MyPortal.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPortalServices(this IServiceCollection services)
    {
        services.AddServices();

        services.AddAccessPolicies();
        
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IBulletinService, BulletinService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IDirectoryEntityService<Bulletin>, BulletinService>();
    }

    private static void AddAccessPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAccessPolicy<Bulletin, BulletinVisibilityScope>, BulletinAccessPolicy>();
    }
}