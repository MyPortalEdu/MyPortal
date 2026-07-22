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
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IAddressPersonRepository, AddressPersonRepository>();
        services.AddScoped<IAddressTypeRepository, AddressTypeRepository>();
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
        services.AddScoped<IEmailAddressRepository, EmailAddressRepository>();
        services.AddScoped<IEmailAddressTypeRepository, EmailAddressTypeRepository>();
        services.AddScoped<IPhoneNumberRepository, PhoneNumberRepository>();
        services.AddScoped<IPhoneNumberTypeRepository, PhoneNumberTypeRepository>();
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
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IEthnicityRepository, EthnicityRepository>();
        services.AddScoped<INationalityRepository, NationalityRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IMaritalStatusRepository, MaritalStatusRepository>();
        services.AddScoped<IReligionRepository, ReligionRepository>();
        services.AddScoped<ISexualOrientationRepository, SexualOrientationRepository>();
        services.AddScoped<IGenderIdentityRepository, GenderIdentityRepository>();
        services.AddScoped<IDisabilityRepository, DisabilityRepository>();
        services.AddScoped<IStaffMemberDisabilityRepository, StaffMemberDisabilityRepository>();
        services.AddScoped<IMedicalConditionRepository, MedicalConditionRepository>();
        services.AddScoped<IDietaryRequirementRepository, DietaryRequirementRepository>();
        services.AddScoped<IPersonConditionRepository, PersonConditionRepository>();
        services.AddScoped<IPersonDietaryRequirementRepository, PersonDietaryRequirementRepository>();
        services.AddScoped<IPersonDisabilityRepository, PersonDisabilityRepository>();
        services.AddScoped<IStaffQualificationRepository, StaffQualificationRepository>();
        services.AddScoped<IQtsRouteRepository, QtsRouteRepository>();
        services.AddScoped<IInductionStatusRepository, InductionStatusRepository>();
        services.AddScoped<IQualificationLevelRepository, QualificationLevelRepository>();
        services.AddScoped<IClassOfDegreeRepository, ClassOfDegreeRepository>();
        services.AddScoped<IStaffEmploymentRepository, StaffEmploymentRepository>();
        services.AddScoped<IStaffContractRepository, StaffContractRepository>();
        services.AddScoped<ILeavingReasonRepository, LeavingReasonRepository>();
        services.AddScoped<IStaffOriginRepository, StaffOriginRepository>();
        services.AddScoped<IStaffDestinationRepository, StaffDestinationRepository>();
        services.AddScoped<IContractTypeRepository, ContractTypeRepository>();
        services.AddScoped<IStaffPreEmploymentChecksRepository, StaffPreEmploymentChecksRepository>();
        services.AddScoped<IDbsCheckRepository, DbsCheckRepository>();
        services.AddScoped<IDbsCheckTypeRepository, DbsCheckTypeRepository>();
        services.AddScoped<IRightToWorkCheckRepository, RightToWorkCheckRepository>();
        services.AddScoped<IRightToWorkDocumentTypeRepository, RightToWorkDocumentTypeRepository>();
        services.AddScoped<IStaffReferenceRepository, StaffReferenceRepository>();
        services.AddScoped<IReferenceTypeRepository, ReferenceTypeRepository>();
        services.AddScoped<IReferenceStatusRepository, ReferenceStatusRepository>();
        services.AddScoped<IStaffOverseasCheckRepository, StaffOverseasCheckRepository>();
        services.AddScoped<IStaffAbsenceRepository, StaffAbsenceRepository>();
        services.AddScoped<IStaffAbsenceTypeRepository, StaffAbsenceTypeRepository>();
        services.AddScoped<IStaffIllnessTypeRepository, StaffIllnessTypeRepository>();
        services.AddScoped<IStaffCalendarRepository, StaffCalendarRepository>();
        services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<IReviewStatusRepository, ReviewStatusRepository>();
        services.AddScoped<IStaffObjectiveRepository, StaffObjectiveRepository>();
        services.AddScoped<IObjectiveStatusRepository, ObjectiveStatusRepository>();
        services.AddScoped<IObjectiveCategoryRepository, ObjectiveCategoryRepository>();
        services.AddScoped<IObservationRepository, ObservationRepository>();
        services.AddScoped<IObservationOutcomeRepository, ObservationOutcomeRepository>();
        services.AddScoped<ITrainingCertificateRepository, TrainingCertificateRepository>();
        services.AddScoped<ITrainingCourseRepository, TrainingCourseRepository>();
        services.AddScoped<ITrainingCertificateStatusRepository, TrainingCertificateStatusRepository>();
        services.AddScoped<IStaffRoleRepository, StaffRoleRepository>();
        services.AddScoped<IServiceTermRepository, ServiceTermRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPayScaleRepository, PayScaleRepository>();
        services.AddScoped<IPayScalePointRepository, PayScalePointRepository>();
        services.AddScoped<IPayScalePointRateRepository, PayScalePointRateRepository>();
        services.AddScoped<IPayZoneRepository, PayZoneRepository>();
        services.AddScoped<ISpecialSchoolOrganisationRepository, SpecialSchoolOrganisationRepository>();
        services.AddScoped<ISpecialSchoolTypeRepository, SpecialSchoolTypeRepository>();
        services.AddScoped<IRegGroupRepository, RegGroupRepository>();
        services.AddScoped<IRegisterRepository, RegisterRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ISchoolHolidayRepository, SchoolHolidayRepository>();
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IStudentContactRelationshipRepository, StudentContactRelationshipRepository>();
        services.AddScoped<IRelationshipTypeRepository, RelationshipTypeRepository>();
        services.AddScoped<IEnrolmentStatusRepository, EnrolmentStatusRepository>();
        services.AddScoped<IBoarderStatusRepository, BoarderStatusRepository>();
        services.AddScoped<IUpnUnknownReasonRepository, UpnUnknownReasonRepository>();
        services.AddScoped<IEnglishProficiencyRepository, EnglishProficiencyRepository>();
        services.AddScoped<IStudentSenNeedRepository, StudentSenNeedRepository>();
        services.AddScoped<ISenStatusHistoryRepository, SenStatusHistoryRepository>();
        services.AddScoped<ISenProvisionRepository, SenProvisionRepository>();
        services.AddScoped<ISenStatementRepository, SenStatementRepository>();
        services.AddScoped<ISenStatusRepository, SenStatusRepository>();
        services.AddScoped<ISenTypeRepository, SenTypeRepository>();
        services.AddScoped<ISenProvisionTypeRepository, SenProvisionTypeRepository>();
        services.AddScoped<ISenStatutoryAssessmentAgreedRepository, SenStatutoryAssessmentAgreedRepository>();
        services.AddScoped<ISenStatutoryAssessmentOutcomeRepository, SenStatutoryAssessmentOutcomeRepository>();
        services.AddScoped<IStudentCareEpisodeRepository, StudentCareEpisodeRepository>();
        services.AddScoped<IStudentPepRepository, StudentPepRepository>();
        services.AddScoped<IStudentPepContributorRepository, StudentPepContributorRepository>();
        services.AddScoped<IStudentChildProtectionPlanRepository, StudentChildProtectionPlanRepository>();
        services.AddScoped<ILivingArrangementRepository, LivingArrangementRepository>();
        services.AddScoped<IPostLookedAfterArrangementRepository, PostLookedAfterArrangementRepository>();
        services.AddScoped<IServiceChildIndicatorRepository, ServiceChildIndicatorRepository>();
        services.AddScoped<IYoungCarerIndicatorRepository, YoungCarerIndicatorRepository>();
        services.AddScoped<IKinshipCareIndicatorRepository, KinshipCareIndicatorRepository>();
        services.AddScoped<IStudentGroupRepository, StudentGroupRepository>();
        services.AddScoped<IStudentGroupSupervisorRepository, StudentGroupSupervisorRepository>();
        services.AddScoped<ITimetableRepository, TimetableRepository>();
        services.AddScoped<ITimetableRunRepository, TimetableRunRepository>();
        services.AddScoped<ITimetableSourceRepository, TimetableSourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IYearGroupRepository, YearGroupRepository>();
    }
}