using MyPortal.Data.Repositories;
using MyPortal.Services.Interfaces.Repositories;

namespace MyPortal.WebApi.Services;

public static class RepositoryRegistry
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDirectoryRepository, DirectoryRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IBulletinRepository, BulletinRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}