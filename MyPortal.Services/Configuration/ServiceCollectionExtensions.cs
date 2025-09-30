using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Services.Interfaces.Services;
using MyPortal.Services.Services;

namespace MyPortal.Services.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPortalServices(this IServiceCollection services)
    {
        services.AddScoped<ISchoolService, SchoolService>();
        
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}