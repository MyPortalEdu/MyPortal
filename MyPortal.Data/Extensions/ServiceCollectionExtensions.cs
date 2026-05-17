using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories;

namespace MyPortal.Data.Extensions;

public static class ServiceCollectionExtensions
{
    private static bool _dapperHandlersRegistered;

    public static void AddRepositories(this IServiceCollection services)
    {
        // Dapper type handlers are process-global state; the guard keeps
        // double-host setups (tests) idempotent.
        if (!_dapperHandlersRegistered)
        {
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
            _dapperHandlersRegistered = true;
        }

        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IAcademicTermRepository, AcademicTermRepository>();
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IAttendanceCodeRepository, AttendanceCodeRepository>();
        services.AddScoped<IAttendanceMarkRepository, AttendanceMarkRepository>();
        services.AddScoped<IAttendancePeriodRepository, AttendancePeriodRepository>();
        services.AddScoped<IAttendanceWeekRepository, AttendanceWeekRepository>();
        services.AddScoped<IBulletinRepository, BulletinRepository>();
        services.AddScoped<IBulletinCategoryRepository, BulletinCategoryRepository>();
        services.AddScoped<IBulletinAcknowledgementRepository, BulletinAcknowledgementRepository>();
        services.AddScoped<IBulletinSettingsRepository, BulletinSettingsRepository>();
        services.AddScoped<IDiaryEventRepository, DiaryEventRepository>();
        services.AddScoped<IDirectoryRepository, DirectoryRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
        services.AddScoped<IHouseRepository, HouseRepository>();
        services.AddScoped<IAgencyTypeRepository, AgencyTypeRepository>();
        services.AddScoped<IGovernanceTypeRepository, GovernanceTypeRepository>();
        services.AddScoped<IIntakeTypeRepository, IntakeTypeRepository>();
        services.AddScoped<ILocalAuthorityRepository, LocalAuthorityRepository>();
        services.AddScoped<ISchoolPhaseRepository, SchoolPhaseRepository>();
        services.AddScoped<ISchoolTypeRepository, SchoolTypeRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IRegGroupRepository, RegGroupRepository>();
        services.AddScoped<IRegisterRepository, RegisterRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ISchoolHolidayRepository, SchoolHolidayRepository>();
        services.AddScoped<IStudentGroupRepository, StudentGroupRepository>();
        services.AddScoped<IStudentGroupSupervisorRepository, StudentGroupSupervisorRepository>();
        services.AddScoped<ITimetableRepository, TimetableRepository>();
        services.AddScoped<ITimetableRunRepository, TimetableRunRepository>();
        services.AddScoped<ITimetableSourceRepository, TimetableSourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IYearGroupRepository, YearGroupRepository>();
    }
}