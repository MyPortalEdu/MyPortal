using MyPortal.Data.Repositories;
using MyPortal.Services.Interfaces.Repositories;

namespace MyPortal.WebApi.Services;

public static class RepositoryRegistry
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}