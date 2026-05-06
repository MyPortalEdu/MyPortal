using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Auth.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Attendance;
using MyPortal.Services.Curriculum;
using MyPortal.Services.Curriculum.Timetable;
using MyPortal.Services.Documents;
using MyPortal.Services.Pastoral;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Attendance;
using MyPortal.Services.Interfaces.Curriculum;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.Pastoral;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.Interfaces.School;
using MyPortal.Services.Interfaces.Security;
using MyPortal.Services.Interfaces.System;
using MyPortal.Services.Interfaces.Timetable;
using MyPortal.Services.People;
using MyPortal.Services.Providers;
using MyPortal.Services.School;
using MyPortal.Services.School.Bulletins;
using MyPortal.Services.Security;
using MyPortal.Services.System;
using MyPortal.Services.Validation;
using MyPortal.Timetabler.Solver;

namespace MyPortal.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPortalServices(this IServiceCollection services)
    {
        services.AddSingletons();
        services.AddProviders();
        services.AddTimetabler();
        services.AddServices();
        services.AddAccessPolicies();
        services.AddValidators();
        
        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IAttendanceCodeService, AttendanceCodeService>();
        services.AddScoped<IAttendanceMarkService, AttendanceMarkService>();
        services.AddScoped<IBulletinService, BulletinService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IHouseService, HouseService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IStudentGroupService, StudentGroupService>();
        services.AddScoped<ITimetableMaterialisationService, TimetableMaterialisationService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<ITimetableSolveService, TimetableSolveService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IDirectoryEntityService<Bulletin>, BulletinService>();
    }

    private static void AddProviders(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
    }

    private static void AddTimetabler(this IServiceCollection services)
    {
        services.AddScoped<TimetableInputBuilder>();
        services.AddScoped<ITimetableSolver, CpSatTimetableSolver>();
    }

    private static void AddSingletons(this IServiceCollection services)
    {
        services.AddSingleton<TimetableRunQueue>();
    }

    private static void AddAccessPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAccessPolicy<Bulletin, BulletinVisibilityScope>, BulletinAccessPolicy>();
    }

    private static void AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}