using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Repositories;

namespace MyPortal.Database.Helpers
{
    internal static class DbFactory
    {
        
        #region Repository Map
        
        private static Dictionary<Type, Func<DbUserWithContext, IRepository>> _creators =
            new()
            {
                { typeof(IAcademicTermRepository), dbUser => new AcademicTermRepository(dbUser) },
                { typeof(IAcademicYearRepository), dbUser => new AcademicYearRepository(dbUser) },
                { typeof(IAccountTransactionRepository), dbUser => new AccountTransactionRepository(dbUser) },
                { typeof(IAchievementOutcomeRepository), dbUser => new AchievementOutcomeRepository(dbUser) },
                { typeof(IAchievementRepository), dbUser => new AchievementRepository(dbUser) },
                { typeof(IAchievementTypeRepository), dbUser => new AchievementTypeRepository(dbUser) },
                { typeof(IActivityRepository), dbUser => new ActivityRepository(dbUser) },
                { typeof(IAddressAgencyRepository), dbUser => new AddressAgencyRepository(dbUser) },
                { typeof(IAddressPersonRepository), dbUser => new AddressPersonRepository(dbUser) },
                { typeof(IAddressRepository), dbUser => new AddressRepository(dbUser) },
                { typeof(IAddressTypeRepository), dbUser => new AddressTypeRepository(dbUser) },
                { typeof(IAgencyRepository), dbUser => new AgencyRepository(dbUser) },
                { typeof(IAgentRepository), dbUser => new AgentRepository(dbUser) },
                { typeof(IAgentTypeRepository), dbUser => new AgentTypeRepository(dbUser) },
                { typeof(IAspectRepository), dbUser => new AspectRepository(dbUser) },
                { typeof(IAspectTypeRepository), dbUser => new AspectTypeRepository(dbUser) },
                { typeof(IAttendanceCodeRepository), dbUser => new AttendanceCodeRepository(dbUser) },
                { typeof(IAttendanceCodeTypeRepository), dbUser => new AttendanceCodeTypeRepository(dbUser) },
                { typeof(IAttendanceMarkRepository), dbUser => new AttendanceMarkRepository(dbUser) },
                { typeof(IAttendancePeriodRepository), dbUser => new AttendancePeriodRepository(dbUser) },
                { typeof(IAttendanceWeekPatternRepository), dbUser => new AttendanceWeekPatternRepository(dbUser) },
                { typeof(IAttendanceWeekRepository), dbUser => new AttendanceWeekRepository(dbUser) },
                { typeof(IBasketItemRepository), dbUser => new BasketItemRepository(dbUser) },
                { typeof(IBehaviourOutcomeRepository), dbUser => new BehaviourOutcomeRepository(dbUser) },
                { typeof(IBehaviourRoleTypeRepository), dbUser => new BehaviourRoleTypeRepository(dbUser) },
                { typeof(IBehaviourStatusRepository), dbUser => new BehaviourStatusRepository(dbUser) },
                { typeof(IBehaviourTargetRepository), dbUser => new BehaviourTargetRepository(dbUser) },
                { typeof(IBillAccountTransactionRepository), dbUser => new BillAccountTransactionRepository(dbUser) },
                { typeof(IBillDiscountRepository), dbUser => new BillDiscountRepository(dbUser) },
                { typeof(IBillItemRepository), dbUser => new BillItemRepository(dbUser) },
                { typeof(IBillRepository), dbUser => new BillRepository(dbUser) },
                { typeof(IBillStudentChargeRepository), dbUser => new BillStudentStudentChargeRepository(dbUser) },
                { typeof(IBoarderStatusRepository), dbUser => new BoarderStatusRepository(dbUser) },
                { typeof(IBuildingFloorRepository), dbUser => new BuildingFloorRepository(dbUser) },
                { typeof(IBuildingRepository), dbUser => new BuildingRepository(dbUser) },
                { typeof(IBulletinRepository), dbUser => new BulletinRepository(dbUser) },
                { typeof(IChargeBillingPeriodRepository), dbUser => new ChargeBillingPeriodRepository(dbUser) },
                { typeof(IChargeDiscountRepository), dbUser => new ChargeDiscountRepository(dbUser) },
                { typeof(IChargeRepository), dbUser => new ChargeRepository(dbUser) },
                { typeof(IClassRepository), dbUser => new ClassRepository(dbUser) },
                { typeof(ICommentBankAreaRepository), dbUser => new CommentBankAreaRepository(dbUser) },
                { typeof(ICommentBankRepository), dbUser =>  new CommentBankRepository(dbUser) },
                { typeof(ICommentBankSectionRepository), dbUser => new CommentBankSectionRepository(dbUser) },
                { typeof(ICommentRepository), dbUser => new CommentRepository(dbUser) },
                { typeof(ICommunicationLogRepository), dbUser => new CommunicationLogRepository(dbUser) },
                { typeof(ICommunicationTypeRepository), dbUser => new CommunicationTypeRepository(dbUser) },
                { typeof(IContactRepository), dbUser => new ContactRepository(dbUser) },
                { typeof(ICourseRepository), dbUser => new CourseRepository(dbUser) },
                { typeof(ICoverArrangementRepository), dbUser => new CoverArrangementRepository(dbUser) },
                { typeof(ICurriculumBandBlockAssignmentRepository), dbUser => new CurriculumBandBlockAssignmentRepository(dbUser) },
                { typeof(ICurriculumBandRepository), dbUser => new CurriculumBandRepository(dbUser) },
                { typeof(ICurriculumBlockRepository), dbUser => new CurriculumBlockRepository(dbUser) },
                { typeof(ICurriculumGroupRepository), dbUser => new CurriculumGroupRepository(dbUser) },
                { typeof(ICurriculumYearGroupRepository), dbUser => new CurriculumYearGroupRepository(dbUser) },
                { typeof(IDetentionRepository), dbUser => new DetentionRepository(dbUser) },
                { typeof(IDetentionTypeRepository), dbUser => new DetentionTypeRepository(dbUser) },
                { typeof(IDiaryEventAttendeeRepository), dbUser => new DiaryEventAttendeeRepository(dbUser) },
                { typeof(IDiaryEventAttendeeResponseRepository), dbUser => new DiaryEventAttendeeResponseRepository(dbUser) },
                { typeof(IDiaryEventRepository), dbUser => new DiaryEventRepository(dbUser) },
                { typeof(IDiaryEventTemplateRepository), dbUser => new DiaryEventTemplateRepository(dbUser) },
                { typeof(IDiaryEventTypeRepository), dbUser => new DiaryEventTypeRepository(dbUser) },
                { typeof(IDietaryRequirementRepository), dbUser => new DietaryRequirementRepository(dbUser) },
                { typeof(IDirectoryRepository), dbUser => new DirectoryRepository(dbUser) },
                { typeof(IDiscountRepository), dbUser => new DiscountRepository(dbUser) },
                { typeof(IDocumentRepository), dbUser => new DocumentRepository(dbUser) },
                { typeof(IDocumentTypeRepository), dbUser => new DocumentTypeRepository(dbUser) },
                { typeof(IEmailAddressRepository), dbUser => new EmailAddressRepository(dbUser) },
                { typeof(IEmailAddressTypeRepository), dbUser => new EmailAddressTypeRepository(dbUser) },
                { typeof(IEnrolmentStatusRepository), dbUser => new EnrolmentStatusRepository(dbUser) },
                { typeof(IEthnicityRepository), dbUser => new EthnicityRepository(dbUser) },
                { typeof(IExamAssessmentAspectRepository), dbUser => new ExamAssessmentAspectRepository(dbUser) },
                { typeof(IExamAssessmentModeRepository), dbUser => new ExamAssessmentModeRepository(dbUser) },
                { typeof(IExamAssessmentRepository), dbUser => new ExamAssessmentRepository(dbUser) },
                { typeof(IExamAwardElementRepository), dbUser => new ExamAwardElementRepository(dbUser) },
                { typeof(IExamAwardRepository), dbUser => new ExamAwardRepository(dbUser) },
                { typeof(IExamAwardSeriesRepository), dbUser => new ExamAwardSeriesRepository(dbUser) },
                { typeof(IExamBaseComponentRepository), dbUser => new ExamBaseComponentRepository(dbUser) },
                { typeof(IExamBaseElementRepository), dbUser => new ExamBaseElementRepository(dbUser) },
                { typeof(IExamBoardRepository), dbUser => new ExamBoardRepository(dbUser) },
                { typeof(IExamCandidateRepository), dbUser => new ExamCandidateRepository(dbUser) },
                { typeof(IExamCandidateSeriesRepository), dbUser => new ExamCandidateSeriesRepository(dbUser) },
                { typeof(IExamCandidateSpecialArrangementRepository), dbUser => new ExamCandidateSpecialArrangementRepository(dbUser) },
                { typeof(IExamComponentRepository), dbUser => new ExamComponentRepository(dbUser) },
                { typeof(IExamComponentSittingRepository), dbUser => new ExamComponentSittingRepository(dbUser) },
                { typeof(IExamDateRepository), dbUser => new ExamDateRepository(dbUser) },
                { typeof(IExamElementComponentRepository), dbUser => new ExamElementComponentRepository(dbUser) },
                { typeof(IExamElementRepository), dbUser => new ExamElementRepository(dbUser) },
                { typeof(IExamEnrolmentRepository), dbUser => new ExamEnrolmentRepository(dbUser) },
                { typeof(IExamQualificationLevelRepository), dbUser => new ExamQualificationLevelRepository(dbUser) },
                { typeof(IExamQualificationRepository), dbUser => new ExamQualificationRepository(dbUser) },
                { typeof(IExamResultEmbargoRepository), dbUser => new ExamResultEmbargoRepository(dbUser) },
                { typeof(IExamRoomRepository), dbUser => new ExamRoomRepository(dbUser) },
                { typeof(IExamRoomSeatBlockRepository), dbUser => new ExamRoomSeatBlockRepository(dbUser) },
                { typeof(IExamSeasonRepository), dbUser => new ExamSeasonRepository(dbUser) },
                { typeof(IExamSeatAllocationRepository), dbUser => new ExamSeatAllocationRepository(dbUser) },
                { typeof(IExamSeriesRepository), dbUser => new ExamSeriesRepository(dbUser) },
                { typeof(IExamSessionRepository), dbUser => new ExamSessionRepository(dbUser) },
                { typeof(IExamSpecialArrangementRepository), dbUser => new ExamSpecialArrangementRepository(dbUser) },
                { typeof(IExclusionAppealResultRepository), dbUser => new ExclusionAppealResultRepository(dbUser) },
                { typeof(IExclusionReasonRepository), dbUser => new ExclusionReasonRepository(dbUser) },
                { typeof(IExclusionRepository), dbUser => new ExclusionRepository(dbUser) },
                { typeof(IExclusionTypeRepository), dbUser => new ExclusionTypeRepository(dbUser) },
                { typeof(IFileRepository), dbUser => new FileRepository(dbUser) },
                { typeof(IGiftedTalentedRepository), dbUser => new GiftedTalentedRepository(dbUser) },
                { typeof(IGovernanceTypeRepository), dbUser => new GovernanceTypeRepository(dbUser) },
                { typeof(IGradeRepository), dbUser => new GradeRepository(dbUser) },
                { typeof(IGradeSetRepository), dbUser => new GradeSetRepository(dbUser) },
                { typeof(IHomeworkItemRepository), dbUser => new HomeworkItemRepository(dbUser) },
                { typeof(IHomeworkSubmissionRepository), dbUser => new HomeworkSubmissionRepository(dbUser) },
                { typeof(IHouseRepository), dbUser => new HouseRepository(dbUser) },
                { typeof(IIncidentRepository), dbUser => new IncidentRepository(dbUser) },
                { typeof(IIncidentTypeRepository), dbUser => new IncidentTypeRepository(dbUser) },
                { typeof(IIntakeTypeRepository), dbUser => new IntakeTypeRepository(dbUser) },
                { typeof(ILanguageRepository), dbUser => new LanguageRepository(dbUser) },
                { typeof(ILessonPlanHomeworkItemRepository), dbUser => new LessonPlanHomeworkItemRepository(dbUser) },
                { typeof(ILessonPlanRepository), dbUser => new LessonPlanRepository(dbUser) },
                { typeof(ILessonPlanTemplateRepository), dbUser => new LessonPlanTemplateRepository(dbUser) },
                { typeof(ILocalAuthorityRepository), dbUser => new LocalAuthorityRepository(dbUser) },
                { typeof(ILogNoteRepository), dbUser => new LogNoteRepository(dbUser) },
                { typeof(ILogNoteTypeRepository), dbUser => new LogNoteTypeRepository(dbUser) },
                { typeof(IMarksheetColumnRepository), dbUser => new MarksheetColumnRepository(dbUser) },
                { typeof(IMarksheetRepository), dbUser => new MarksheetRepository(dbUser) },
                { typeof(IMarksheetTemplateRepository), dbUser => new MarksheetTemplateRepository(dbUser) },
                { typeof(IMedicalConditionRepository), dbUser => new MedicalConditionRepository(dbUser) },
                { typeof(IMedicalEventRepository), dbUser => new MedicalEventRepository(dbUser) },
                { typeof(INextOfKinRelationshipTypeRepository), dbUser => new NextOfKinRelationshipTypeRepository(dbUser) },
                { typeof(INextOfKinRepository), dbUser => new NextOfKinRepository(dbUser) },
                { typeof(IObservationOutcomeRepository), dbUser => new ObservationOutcomeRepository(dbUser) },
                { typeof(IObservationRepository), dbUser => new ObservationRepository(dbUser) },
                { typeof(IParentEveningAppointmentRepository), dbUser => new ParentEveningAppointmentRepository(dbUser) },
                { typeof(IParentEveningBreakRepository), dbUser => new ParentEveningBreakRepository(dbUser) },
                { typeof(IParentEveningGroupRepository), dbUser => new ParentEveningGroupRepository(dbUser) },
                { typeof(IParentEveningRepository), dbUser => new ParentEveningRepository(dbUser) },
                { typeof(IParentEveningStaffMemberRepository), dbUser => new ParentEveningStaffMemberRepository(dbUser) },
                { typeof(IPersonConditionRepository), dbUser => new PersonConditionRepository(dbUser) },
                { typeof(IPersonDietaryRequirementRepository), dbUser => new PersonDietaryRequirementRepository(dbUser) },
                { typeof(IPersonRepository), dbUser => new PersonRepository(dbUser) },
                { typeof(IPhoneNumberRepository), dbUser => new PhoneNumberRepository(dbUser) },
                { typeof(IPhoneNumberTypeRepository), dbUser => new PhoneNumberTypeRepository(dbUser) },
                { typeof(IPhotoRepository), dbUser => new PhotoRepository(dbUser) },
                { typeof(IProductRepository), dbUser => new ProductRepository(dbUser) },
                { typeof(IProductTypeRepository), dbUser => new ProductTypeRepository(dbUser) },
                { typeof(IRegGroupRepository), dbUser => new RegGroupRepository(dbUser) },
                { typeof(IRelationshipTypeRepository), dbUser => new RelationshipTypeRepository(dbUser) },
                { typeof(IReportCardEntryRepository), dbUser => new ReportCardEntryRepository(dbUser) },
                { typeof(IReportCardRepository), dbUser => new ReportCardRepository(dbUser) },
                { typeof(IReportCardTargetEntryRepository), dbUser => new ReportCardTargetEntryRepository(dbUser) },
                { typeof(IReportCardTargetRepository), dbUser => new ReportCardTargetRepository(dbUser) },
                { typeof(IResultRepository), dbUser => new ResultRepository(dbUser) },
                { typeof(IResultSetRepository), dbUser => new ResultSetRepository(dbUser) },
                { typeof(IRoleRepository), dbUser => new RoleRepository(dbUser) },
                { typeof(IRoomClosureReasonRepository), dbUser => new RoomClosureReasonRepository(dbUser) }
            };
        
        #endregion
        
        internal static TInterface CreateRepository<TInterface>(DbUserWithContext dbUser) where TInterface : IRepository
        {
            if (_creators.TryGetValue(typeof(TInterface), out var creator))
            {
                return (TInterface)creator(dbUser);
            }

            throw new InvalidOperationException($"No repository registered for {typeof(TInterface).Name}");
        }

        
        internal static ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MyPortalConnection", EnvironmentVariableTarget.Machine) ??
                string.Empty;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}