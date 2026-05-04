using Microsoft.Extensions.DependencyInjection;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories;

namespace MyPortal.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IAcademicTermRepository, AcademicTermRepository>();
        services.AddScoped<IBulletinRepository, BulletinRepository>();
        services.AddScoped<IDirectoryRepository, DirectoryRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IRegisterRepository, RegisterRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ITimetableRepository, TimetableRepository>();
        services.AddScoped<ITimetableRunRepository, TimetableRunRepository>();
        services.AddScoped<ITimetableSourceRepository, TimetableSourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}