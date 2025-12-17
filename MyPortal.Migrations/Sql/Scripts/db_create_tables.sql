SET XACT_ABORT ON;
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo._DatabaseUpdates', N'U') IS NULL
BEGIN
CREATE TABLE dbo._DatabaseUpdates
(
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ScriptName    NVARCHAR(260)     NOT NULL,
    AppliedOn     DATETIME2(0)      NOT NULL CONSTRAINT DF__DbUpdates_AppliedOn DEFAULT SYSUTCDATETIME(),
    AppliedBy     NVARCHAR(128)     NOT NULL CONSTRAINT DF__DbUpdates_AppliedBy DEFAULT SUSER_SNAME(),
    ExecutionMs   INT               NULL,
    ScriptHash    VARBINARY(32)     NULL,
    Success       BIT               NOT NULL CONSTRAINT DF__DbUpdates_Success DEFAULT (1),
    ErrorMessage  NVARCHAR(4000)    NULL
);
CREATE UNIQUE INDEX UX__DbUpdates_ScriptName_Success
    ON dbo._DatabaseUpdates (ScriptName)
    WHERE Success = 1;
END;

-- === CREATE TABLES ===

IF OBJECT_ID(N'[dbo].[AcademicTerms]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AcademicTerms] (
    [Id] uniqueidentifier NOT NULL,
    [AcademicYearId] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    CONSTRAINT PK_AcademicTerms PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AcademicYears]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AcademicYears] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [IsLocked] bit NOT NULL,
    CONSTRAINT PK_AcademicYears PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AccountTransactions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AccountTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Date] datetime2(7) NOT NULL,
    CONSTRAINT PK_AccountTransactions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AchievementOutcomes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AchievementOutcomes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AchievementOutcomes_Active DEFAULT (1),
    CONSTRAINT PK_AchievementOutcomes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AchievementTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AchievementTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AchievementTypes_Active DEFAULT (1),
    [DefaultPoints] int NOT NULL,
    CONSTRAINT PK_AchievementTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Achievements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Achievements] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Achievements_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Achievements_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [AcademicYearId] uniqueidentifier NOT NULL,
    [AchievementTypeId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NULL,
    [Date] datetime2(7) NOT NULL,
    [Comments] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Achievements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Activities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Activities] (
    [Id] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_Activities PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AddressAgencies]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AddressAgencies] (
    [Id] uniqueidentifier NOT NULL,
    [AddressId] uniqueidentifier NOT NULL,
    [AgencyId] uniqueidentifier NOT NULL,
    [AddressTypeId] uniqueidentifier NOT NULL,
    [IsMain] bit NOT NULL,
     CONSTRAINT PK_AddressAgencies PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AddressPeople]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AddressPeople] (
    [Id] uniqueidentifier NOT NULL,
    [AddressId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NULL,
    [AddressTypeId] uniqueidentifier NOT NULL,
    [IsMain] bit NOT NULL,
     CONSTRAINT PK_AddressPeople PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AddressTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AddressTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AddressTypes_Active DEFAULT (1),
    CONSTRAINT PK_AddressTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Addresses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Addresses] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingNumber] nvarchar(128) NULL,
    [BuildingName] nvarchar(128) NULL,
    [Apartment] nvarchar(128) NULL,
    [Street] nvarchar(256) NOT NULL,
    [District] nvarchar(256) NULL,
    [Town] nvarchar(256) NOT NULL,
    [County] nvarchar(256) NOT NULL,
    [Postcode] nvarchar(256) NOT NULL,
    [Country] nvarchar(256) NOT NULL,
    [IsValidated] bit NOT NULL,
    CONSTRAINT PK_Addresses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Agencies]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Agencies] (
    [Id] uniqueidentifier NOT NULL,
    [AgencyTypeId] uniqueidentifier NOT NULL,
    [DirectoryId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Website] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Agencies PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AgencyTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AgencyTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AgencyTypes_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_AgencyTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AgentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AgentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AgentTypes_Active DEFAULT (1),
    CONSTRAINT PK_AgentTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Agents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Agents] (
    [Id] uniqueidentifier NOT NULL,
    [AgencyId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [AgentTypeId] uniqueidentifier NOT NULL,
    [JobTitle] nvarchar(128) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Agents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AspectTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AspectTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AspectTypes_Active DEFAULT (1),
    CONSTRAINT PK_AspectTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Aspects]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Aspects] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Aspects_Active DEFAULT (1),
    [TypeId] uniqueidentifier NOT NULL,
    [GradeSetId] uniqueidentifier NULL,
    [MinMark] decimal(18,2) NULL,
    [MaxMark] decimal(18,2) NULL,
    [Name] nvarchar(256) NOT NULL,
    [ColumnHeading] nvarchar(256) NOT NULL,
    [IsPrivate] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_Aspects PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendanceCodeTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendanceCodeTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    CONSTRAINT PK_AttendanceCodeTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendanceCodes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendanceCodes] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [AttendanceCodeTypeId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [IsRestricted] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_AttendanceCodes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendanceMarks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendanceMarks] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_AttendanceMarks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_AttendanceMarks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [StudentId] uniqueidentifier NOT NULL,
    [AttendanceWeekId] uniqueidentifier NOT NULL,
    [AttendancePeriodId] uniqueidentifier NOT NULL,
    [AttendanceCodeId] uniqueidentifier NOT NULL,
    [Comments] nvarchar(256) NULL,
    [MinutesLate] int NULL,
    CONSTRAINT PK_AttendanceMarks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendancePeriods]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendancePeriods] (
    [Id] uniqueidentifier NOT NULL,
    [WeekPatternId] uniqueidentifier NOT NULL,
    [Weekday] nvarchar(256) NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [StartTime] time(7) NOT NULL,
    [EndTime] time(7) NOT NULL,
    [IsAmReg] bit NOT NULL,
    [IsPmReg] bit NOT NULL,
    CONSTRAINT PK_AttendancePeriods PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendanceWeekPatterns]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendanceWeekPatterns] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    CONSTRAINT PK_AttendanceWeekPatterns PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[AttendanceWeeks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AttendanceWeeks] (
    [Id] uniqueidentifier NOT NULL,
    [WeekPatternId] uniqueidentifier NOT NULL,
    [AcademicTermId] uniqueidentifier NOT NULL,
    [Beginning] datetime2(7) NOT NULL,
    [IsNonTimetable] bit NOT NULL,
    CONSTRAINT PK_AttendanceWeeks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BasketItems]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BasketItems] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
     CONSTRAINT PK_BasketItems PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BehaviourOutcomes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BehaviourOutcomes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BehaviourOutcomes_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_BehaviourOutcomes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BehaviourRoleTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BehaviourRoleTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BehaviourRoleTypes_Active DEFAULT (1),
    [DefaultPoints] int NOT NULL,
    CONSTRAINT PK_BehaviourRoleTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BehaviourStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BehaviourStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BehaviourStatus_Active DEFAULT (1),
    [IsResolved] bit NOT NULL,
    CONSTRAINT PK_BehaviourStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BehaviourTargets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BehaviourTargets] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BehaviourTargets_Active DEFAULT (1),
    CONSTRAINT PK_BehaviourTargets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BillAccountTransactions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BillAccountTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [BillId] uniqueidentifier NOT NULL,
    [AccountTransactionId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_BillAccountTransactions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BillCharges]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BillCharges] (
    [Id] uniqueidentifier NOT NULL,
    [BillId] uniqueidentifier NOT NULL,
    [StudentChargeId] uniqueidentifier NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [VatAmount] decimal(18,2) NOT NULL,
    CONSTRAINT PK_BillCharges PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BillDiscounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BillDiscounts] (
    [Id] uniqueidentifier NOT NULL,
    [BillId] uniqueidentifier NOT NULL,
    [DiscountId] uniqueidentifier NOT NULL,
    [GrossAmount] decimal(18,2) NOT NULL,
    CONSTRAINT PK_BillDiscounts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BillItems]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BillItems] (
    [Id] uniqueidentifier NOT NULL,
    [BillId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [VatAmount] decimal(18,2) NOT NULL,
    [CustomerReceived] bit NOT NULL,
    [IsRefunded] bit NOT NULL,
    CONSTRAINT PK_BillItems PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Bills]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bills] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Bills_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Bills_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [StudentId] uniqueidentifier NOT NULL,
    [ChargeBillingPeriodId] uniqueidentifier NULL,
    [DueDate] datetime2(7) NOT NULL,
    [IsDispatched] bit NULL,
    CONSTRAINT PK_Bills PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BoarderStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BoarderStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BoarderStatus_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    CONSTRAINT PK_BoarderStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[BuildingFloors]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BuildingFloors] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_BuildingFloors_Active DEFAULT (1),
    [BuildingId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_BuildingFloors PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Buildings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Buildings] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Buildings_Active DEFAULT (1),
    CONSTRAINT PK_Buildings PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Bulletins]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bulletins] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Bulletins_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Bulletins_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [DirectoryId] uniqueidentifier NOT NULL,
    [ExpiresAt] datetime2(7) NULL,
    [Title] nvarchar(256) NOT NULL,
    [Detail] nvarchar(256) NOT NULL,
    [IsPrivate] bit NOT NULL,
    [IsApproved] bit NOT NULL,
    CONSTRAINT PK_Bulletins PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ChargeBillingPeriods]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ChargeBillingPeriods] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    CONSTRAINT PK_ChargeBillingPeriods PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ChargeDiscounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ChargeDiscounts] (
    [Id] uniqueidentifier NOT NULL,
    [ChargeId] uniqueidentifier NOT NULL,
    [DiscountId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ChargeDiscounts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Charges]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Charges] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Charges_Active DEFAULT (1),
    [VatRateId] uniqueidentifier NOT NULL,
    [Code] nvarchar(64) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [IsVariable] bit NOT NULL,
    CONSTRAINT PK_Charges PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Classes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Classes] (
    [Id] uniqueidentifier NOT NULL,
    [CourseId] uniqueidentifier NOT NULL,
    [CurriculumGroupId] uniqueidentifier NOT NULL,
    [DirectoryId] uniqueidentifier NOT NULL,
    [Code] nvarchar(256) NOT NULL,
    CONSTRAINT PK_Classes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommentBankAreas]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommentBankAreas] (
    [Id] uniqueidentifier NOT NULL,
    [CommentBankId] uniqueidentifier NOT NULL,
    [CourseId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    CONSTRAINT PK_CommentBankAreas PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommentBankSections]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommentBankSections] (
    [Id] uniqueidentifier NOT NULL,
    [CommentBankAreaId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    CONSTRAINT PK_CommentBankSections PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommentBanks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommentBanks] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_CommentBanks_Active DEFAULT (1),
    CONSTRAINT PK_CommentBanks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_CommentTypes_Active DEFAULT (1),
    CONSTRAINT PK_CommentTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Comments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Comments] (
    [Id] uniqueidentifier NOT NULL,
    [CommentTypeId] uniqueidentifier NOT NULL,
    [CommentBankSectionId] uniqueidentifier NOT NULL,
    [Value] nvarchar(256) NOT NULL,
    CONSTRAINT PK_Comments PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommunicationLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommunicationLogs] (
    [Id] uniqueidentifier NOT NULL,
    [ContactId] uniqueidentifier NOT NULL,
    [CommunicationTypeId] uniqueidentifier NOT NULL,
    [Date] datetime2(7) NOT NULL,
    [Notes] nvarchar(256) NULL,
    [IsOuting] bit NOT NULL,
    CONSTRAINT PK_CommunicationLogs PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CommunicationTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CommunicationTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_CommunicationTypes_Active DEFAULT (1),
    CONSTRAINT PK_CommunicationTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Contacts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Contacts] (
    [Id] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [ParentalBallot] bit NOT NULL,
    [PlaceOfWork] nvarchar(256) NULL,
    [JobTitle] nvarchar(256) NULL,
    [NiNumber] nvarchar(128) NULL,
    CONSTRAINT PK_Contacts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Courses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Courses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Courses_Active DEFAULT (1),
    [SubjectId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    CONSTRAINT PK_Courses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CoverArrangements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CoverArrangements] (
    [Id] uniqueidentifier NOT NULL,
    [WeekId] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [TeacherId] uniqueidentifier NULL,
    [RoomId] uniqueidentifier NULL,
    [Comments] nvarchar(256) NULL,
    CONSTRAINT PK_CoverArrangements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumBandBlockAssignments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumBandBlockAssignments] (
    [Id] uniqueidentifier NOT NULL,
    [BlockId] uniqueidentifier NOT NULL,
    [BandId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_CurriculumBandBlockAssignments PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumBands]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumBands] (
    [Id] uniqueidentifier NOT NULL,
    [AcademicYearId] uniqueidentifier NOT NULL,
    [CurriculumYearGroupId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_CurriculumBands PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumBlocks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumBlocks] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    [Description] nvarchar(256) NULL,
    CONSTRAINT PK_CurriculumBlocks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumGroupSessions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumGroupSessions] (
    [Id] uniqueidentifier NOT NULL,
    [CurriculumGroupId] uniqueidentifier NOT NULL,
    [SubjectId] uniqueidentifier NOT NULL,
    [SessionTypeId] uniqueidentifier NOT NULL,
    [SessionAmount] int NOT NULL,
     CONSTRAINT PK_CurriculumGroupSessions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumGroups] (
    [Id] uniqueidentifier NOT NULL,
    [BlockId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_CurriculumGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[CurriculumYearGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[CurriculumYearGroups] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [KeyStage] int NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_CurriculumYearGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DetentionTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DetentionTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DetentionTypes_Active DEFAULT (1),
    [StartTime] time(7) NOT NULL,
    [EndTime] time(7) NOT NULL,
    CONSTRAINT PK_DetentionTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Detentions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Detentions] (
    [Id] uniqueidentifier NOT NULL,
    [DetentionTypeId] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [SupervisorId] uniqueidentifier NULL,
     CONSTRAINT PK_Detentions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DiaryEventAttendeeResponses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DiaryEventAttendeeResponses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DiaryEventAttendeeResponses_Active DEFAULT (1),
    CONSTRAINT PK_DiaryEventAttendeeResponses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DiaryEventAttendees]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DiaryEventAttendees] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [ResponseId] uniqueidentifier NULL,
    [IsRequired] bit NOT NULL,
    [HasAttended] bit NULL,
    [CanEditEvent] bit NOT NULL,
     CONSTRAINT PK_DiaryEventAttendees PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DiaryEventTemplates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DiaryEventTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DiaryEventTemplates_Active DEFAULT (1),
    [DiaryEventTypeId] uniqueidentifier NOT NULL,
    [Minutes] int NOT NULL,
    [Hours] int NOT NULL,
    [Days] int NOT NULL,
    CONSTRAINT PK_DiaryEventTemplates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DiaryEventTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DiaryEventTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DiaryEventTypes_Active DEFAULT (1),
    [ColourCode] nvarchar(128) NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_DiaryEventTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DiaryEvents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DiaryEvents] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_DiaryEvents_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_DiaryEvents_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [EventTypeId] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NULL,
    [Subject] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [Location] nvarchar(256) NULL,
    [StartTime] datetime2(7) NOT NULL,
    [EndTime] datetime2(7) NOT NULL,
    [IsAllDay] bit NOT NULL,
    [IsPublic] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_DiaryEvents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DietaryRequirements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DietaryRequirements] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DietaryRequirements_Active DEFAULT (1),
    CONSTRAINT PK_DietaryRequirements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Directories]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Directories] (
    [Id] uniqueidentifier NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [Name] nvarchar(256) NOT NULL,
    [IsPrivate] bit NOT NULL,
    CONSTRAINT PK_Directories PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Discounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Discounts] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Discounts_Active DEFAULT (1),
    [Name] nvarchar(128) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [IsPercentage] bit NOT NULL,
    [BlockOtherDiscounts] bit NOT NULL,
    CONSTRAINT PK_Discounts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[DocumentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DocumentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DocumentTypes_Active DEFAULT (1),
    [Staff] bit NOT NULL,
    [Student] bit NOT NULL,
    [Contact] bit NOT NULL,
    [General] bit NOT NULL,
    [IsSend] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_DocumentTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Documents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Documents] (
    -- Audit
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL,   
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL,    

    -- Classification
    [TypeId] uniqueidentifier NOT NULL,
    [DirectoryId] uniqueidentifier NOT NULL,

    -- File-related columns       
    [StorageKey] nvarchar(512) NOT NULL,
    [FileName] nvarchar(256) NOT NULL,
    [ContentType] nvarchar(256) NOT NULL,
    [SizeBytes] bigint NULL,
    [Hash] nvarchar(128) NULL,

    -- Document-level metadata
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [IsPrivate] bit NOT NULL,
    [IsDeleted] bit NOT NULL,

    CONSTRAINT PK_Documents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[EmailAddressTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[EmailAddressTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_EmailAddressTypes_Active DEFAULT (1),
    CONSTRAINT PK_EmailAddressTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[EmailAddresses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[EmailAddresses] (
    [Id] uniqueidentifier NOT NULL,
    [TypeId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NULL,
    [AgencyId] uniqueidentifier NULL,
    [Address] nvarchar(128) NOT NULL,
    [IsMain] bit NOT NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_EmailAddresses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[EnrolmentStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[EnrolmentStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_EnrolmentStatus_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    CONSTRAINT PK_EnrolmentStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Ethnicities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Ethnicities] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Ethnicities_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_Ethnicities PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAssessmentAspects]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAssessmentAspects] (
    [Id] uniqueidentifier NOT NULL,
    [AssessmentId] uniqueidentifier NOT NULL,
    [AspectId] uniqueidentifier NOT NULL,
    [SeriesId] uniqueidentifier NOT NULL,
    [AspectOrder] int NOT NULL,
     CONSTRAINT PK_ExamAssessmentAspects PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAssessmentModes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAssessmentModes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExamAssessmentModes_Active DEFAULT (1),
    [ExternallyAssessed] bit NOT NULL,
    CONSTRAINT PK_ExamAssessmentModes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAssessments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAssessments] (
    [Id] uniqueidentifier NOT NULL,
    [ExamBoardId] uniqueidentifier NOT NULL,
    [AssessmentType] nvarchar(256) NOT NULL,
    [InternalTitle] nvarchar(256) NULL,
    [ExternalTitle] nvarchar(256) NULL,
    CONSTRAINT PK_ExamAssessments PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAwardElements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAwardElements] (
    [Id] uniqueidentifier NOT NULL,
    [AwardId] uniqueidentifier NOT NULL,
    [ElementId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ExamAwardElements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAwardSeries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAwardSeries] (
    [Id] uniqueidentifier NOT NULL,
    [AwardId] uniqueidentifier NOT NULL,
    [SeriesId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ExamAwardSeries PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamAwards]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamAwards] (
    [Id] uniqueidentifier NOT NULL,
    [QualificationId] uniqueidentifier NOT NULL,
    [AssessmentId] uniqueidentifier NOT NULL,
    [CourseId] uniqueidentifier NULL,
    [Description] nvarchar(256) NULL,
    [AwardCode] nvarchar(256) NULL,
    [ExpiryDate] datetime2(7) NULL,
    CONSTRAINT PK_ExamAwards PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamBaseComponents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamBaseComponents] (
    [Id] uniqueidentifier NOT NULL,
    [AssessmentModeId] uniqueidentifier NOT NULL,
    [ExamAssessmentId] uniqueidentifier NOT NULL,
    [ComponentCode] nvarchar(256) NULL,
    CONSTRAINT PK_ExamBaseComponents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamBaseElements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamBaseElements] (
    [Id] uniqueidentifier NOT NULL,
    [AssessmentId] uniqueidentifier NOT NULL,
    [LevelId] uniqueidentifier NOT NULL,
    [QcaCodeId] uniqueidentifier NOT NULL,
    [QualAccrNumber] nvarchar(256) NULL,
    [ElementCode] nvarchar(256) NULL,
    CONSTRAINT PK_ExamBaseElements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamBoards]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamBoards] (
    [Id] uniqueidentifier NOT NULL,
    [Abbreviation] nvarchar(20) NULL,
    [FullName] nvarchar(128) NULL,
    [Code] nvarchar(5) NULL,
    [IsDomestic] bit NOT NULL,
    [UseEdi] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT PK_ExamBoards PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamCandidate]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamCandidate] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Uci] nvarchar(256) NULL,
    [CandidateNumber] nvarchar(4) NULL,
    [PreviousCandidateNumber] nvarchar(4) NULL,
    [PreviousCentreNumber] nvarchar(5) NULL,
    [SpecialConsideration] bit NOT NULL,
    [Note] nvarchar(256) NULL,
    CONSTRAINT PK_ExamCandidate PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamCandidateSeries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamCandidateSeries] (
    [Id] uniqueidentifier NOT NULL,
    [SeriesId] uniqueidentifier NOT NULL,
    [CandidateId] uniqueidentifier NOT NULL,
    [Flag] nvarchar(256) NULL,
    CONSTRAINT PK_ExamCandidateSeries PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamCandidateSpecialArrangements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamCandidateSpecialArrangements] (
    [Id] uniqueidentifier NOT NULL,
    [CandidateId] uniqueidentifier NOT NULL,
    [SpecialArrangementId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ExamCandidateSpecialArrangements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamComponentSittings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamComponentSittings] (
    [Id] uniqueidentifier NOT NULL,
    [ComponentId] uniqueidentifier NOT NULL,
    [ExamRoomId] uniqueidentifier NOT NULL,
    [ActualStartTime] time(7) NULL,
    [ExtraTimePercent] int NOT NULL,
    [Component] nvarchar(256) NULL,
    [Room] nvarchar(256) NULL,
    CONSTRAINT PK_ExamComponentSittings PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamComponents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamComponents] (
    [Id] uniqueidentifier NOT NULL,
    [BaseComponentId] uniqueidentifier NOT NULL,
    [ExamSeriesId] uniqueidentifier NOT NULL,
    [AssessmentModeId] uniqueidentifier NOT NULL,
    [ExamDateId] uniqueidentifier NULL,
    [DateDue] datetime2(7) NULL,
    [DateSubmitted] datetime2(7) NULL,
    [MaximumMark] int NOT NULL,
    CONSTRAINT PK_ExamComponents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamDates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamDates] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [Duration] int NOT NULL,
    [SittingDate] datetime2(7) NOT NULL,
    CONSTRAINT PK_ExamDates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamElementComponents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamElementComponents] (
    [Id] uniqueidentifier NOT NULL,
    [ElementId] uniqueidentifier NOT NULL,
    [ComponentId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ExamElementComponents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamElements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamElements] (
    [Id] uniqueidentifier NOT NULL,
    [BaseElementId] uniqueidentifier NOT NULL,
    [SeriesId] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NULL,
    [ExamFee] decimal(18,2) NULL,
    [IsSubmitted] bit NOT NULL,
    CONSTRAINT PK_ExamElements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamEnrolments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamEnrolments] (
    [Id] uniqueidentifier NOT NULL,
    [AwardId] uniqueidentifier NOT NULL,
    [CandidateId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [RegistrationNumber] nvarchar(256) NULL,
    CONSTRAINT PK_ExamEnrolments PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamQualificationLevels]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamQualificationLevels] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExamQualificationLevels_Active DEFAULT (1),
    [QualificationId] uniqueidentifier NOT NULL,
    [DefaultGradeSetId] uniqueidentifier NULL,
    [JcLevelCode] nvarchar(25) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExamQualificationLevels PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamQualifications]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamQualifications] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExamQualifications_Active DEFAULT (1),
    [JcQualificationCode] nvarchar(256) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExamQualifications PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamResultEmbares]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamResultEmbares] (
    [Id] uniqueidentifier NOT NULL,
    [ResultSetId] uniqueidentifier NOT NULL,
    [EndTime] datetime2(7) NOT NULL,
    CONSTRAINT PK_ExamResultEmbares PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamRoomSeatBlocks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamRoomSeatBlocks] (
    [Id] uniqueidentifier NOT NULL,
    [ExamRoomId] uniqueidentifier NOT NULL,
    [SeatRow] int NOT NULL,
    [SeatColumn] int NOT NULL,
    [Comments] nvarchar(256) NULL,
    CONSTRAINT PK_ExamRoomSeatBlocks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamRooms]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamRooms] (
    [Id] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NOT NULL,
    [Columns] int NOT NULL,
    [Rows] int NOT NULL,
     CONSTRAINT PK_ExamRooms PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamSeasons]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamSeasons] (
    [Id] uniqueidentifier NOT NULL,
    [ResultSetId] uniqueidentifier NOT NULL,
    [CalendarYear] int NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [IsDefault] bit NOT NULL,
    CONSTRAINT PK_ExamSeasons PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamSeatAllocations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamSeatAllocations] (
    [Id] uniqueidentifier NOT NULL,
    [SittingId] uniqueidentifier NOT NULL,
    [SeatRow] int NOT NULL,
    [SeatColumn] int NOT NULL,
    [CandidateId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [HasAttended] bit NOT NULL,
     CONSTRAINT PK_ExamSeatAllocations PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamSeries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamSeries] (
    [Id] uniqueidentifier NOT NULL,
    [ExamBoardId] uniqueidentifier NOT NULL,
    [ExamSeasonId] uniqueidentifier NOT NULL,
    [SeriesCode] nvarchar(256) NOT NULL,
    [Code] nvarchar(256) NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    CONSTRAINT PK_ExamSeries PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamSessions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamSessions] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExamSessions_Active DEFAULT (1),
    [StartTime] time(7) NOT NULL,
    CONSTRAINT PK_ExamSessions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExamSpecialArrangements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExamSpecialArrangements] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExamSpecialArrangements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExclusionAppealResults]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExclusionAppealResults] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExclusionAppealResults_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExclusionAppealResults PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExclusionReasons]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExclusionReasons] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExclusionReasons_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExclusionReasons PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ExclusionTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ExclusionTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ExclusionTypes_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_ExclusionTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Exclusions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Exclusions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ExclusionTypeId] uniqueidentifier NOT NULL,
    [ExclusionReasonId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Comments] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL,
    [AppealDate] datetime2(7) NULL,
    [AppealResultDate] datetime2(7) NULL,
    [AppealResultId] uniqueidentifier NULL,
    CONSTRAINT PK_Exclusions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[GiftedTalentedStudents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[GiftedTalentedStudents] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SubjectId] uniqueidentifier NOT NULL,
    [Notes] nvarchar(256) NOT NULL,
    CONSTRAINT PK_GiftedTalentedStudents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[GovernanceTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[GovernanceTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_GovernanceTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_GovernanceTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[GradeSets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[GradeSets] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_GradeSets_Active DEFAULT (1),
    [Name] nvarchar(256) NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_GradeSets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Grades]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Grades] (
    [Id] uniqueidentifier NOT NULL,
    [GradeSetId] uniqueidentifier NOT NULL,
    [Code] nvarchar(25) NOT NULL,
    [Description] nvarchar(50) NULL,
    [Value] decimal(18,2) NOT NULL,
    CONSTRAINT PK_Grades PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[HomeworkItems]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[HomeworkItems] (
    [Id] uniqueidentifier NOT NULL,
    [DirectoryId] uniqueidentifier NOT NULL,
    [Title] nvarchar(128) NOT NULL,
    [Description] nvarchar(256) NULL,
    [SubmitOnline] bit NOT NULL,
    [MaxPoints] int NOT NULL,
    CONSTRAINT PK_HomeworkItems PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[HomeworkSubmissions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[HomeworkSubmissions] (
    [Id] uniqueidentifier NOT NULL,
    [HomeworkItemId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TaskId] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NULL,
    [PointsAchieved] int NOT NULL,
    [Comments] nvarchar(256) NULL,
    CONSTRAINT PK_HomeworkSubmissions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Houses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Houses] (
    [Id] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [ColourCode] nvarchar(10) NULL,
    CONSTRAINT PK_Houses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[IncidentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[IncidentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_IncidentTypes_Active DEFAULT (1),
    [DefaultPoints] int NOT NULL,
    CONSTRAINT PK_IncidentTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Incidents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Incidents] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Incidents_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Incidents_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [AcademicYearId] uniqueidentifier NOT NULL,
    [IncidentTypeId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NULL,
    [Date] datetime2(7) NOT NULL,
    [Comments] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Incidents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[IntakeTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[IntakeTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_IntakeTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_IntakeTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Languages]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Languages] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Languages_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    CONSTRAINT PK_Languages PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LessonPlanHomeworkItems]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LessonPlanHomeworkItems] (
    [Id] uniqueidentifier NOT NULL,
    [LessonPlanId] uniqueidentifier NOT NULL,
    [HomeworkItemId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_LessonPlanHomeworkItems PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LessonPlanTemplates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LessonPlanTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [PlanTemplate] nvarchar(256) NOT NULL,
    CONSTRAINT PK_LessonPlanTemplates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LessonPlans]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LessonPlans] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_LessonPlans_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_LessonPlans_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [StudyTopicId] uniqueidentifier NOT NULL,
    [DirectoryId] uniqueidentifier NOT NULL,
    [Order] int NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [PlanContent] nvarchar(256) NOT NULL,
    CONSTRAINT PK_LessonPlans PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LocalAuthorities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LocalAuthorities] (
    [Id] uniqueidentifier NOT NULL,
    [LeaCode] int NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Website] nvarchar(256) NULL,
    CONSTRAINT PK_LocalAuthorities PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Locations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Locations] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(128) NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_Locations PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LogNoteTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LogNoteTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_LogNoteTypes_Active DEFAULT (1),
    [ColourCode] nvarchar(128) NOT NULL,
    [IconClass] nvarchar(256) NOT NULL,
    CONSTRAINT PK_LogNoteTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[LogNotes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LogNotes] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_LogNotes_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_LogNotes_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [LogNoteTypeId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [AcademicYearId] uniqueidentifier NOT NULL,
    [Message] nvarchar(256) NOT NULL,
    [IsPrivate] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_LogNotes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[MarksheetColumns]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[MarksheetColumns] (
    [Id] uniqueidentifier NOT NULL,
    [TemplateId] uniqueidentifier NOT NULL,
    [AspectId] uniqueidentifier NOT NULL,
    [ResultSetId] uniqueidentifier NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsReadOnly] bit NOT NULL,
     CONSTRAINT PK_MarksheetColumns PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[MarksheetTemplates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[MarksheetTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Notes] nvarchar(256) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT PK_MarksheetTemplates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Marksheets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Marksheets] (
    [Id] uniqueidentifier NOT NULL,
    [MarksheetTemplateId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [IsCompleted] bit NOT NULL,
     CONSTRAINT PK_Marksheets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[MedicalConditions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[MedicalConditions] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_MedicalConditions_Active DEFAULT (1),
    CONSTRAINT PK_MedicalConditions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[MedicalEvents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[MedicalEvents] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_MedicalEvents_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_MedicalEvents_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [PersonId] uniqueidentifier NOT NULL,
    [Date] datetime2(7) NOT NULL,
    [Note] nvarchar(256) NOT NULL,
    CONSTRAINT PK_MedicalEvents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[NextOfKin]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[NextOfKin] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [NextOfKinPersonId] uniqueidentifier NOT NULL,
    [RelationshipTypeId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_NextOfKin PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[NextOfKinRelationshipTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[NextOfKinRelationshipTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_NextOfKinRelationshipTypes_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_NextOfKinRelationshipTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ObservationOutcomes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ObservationOutcomes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ObservationOutcomes_Active DEFAULT (1),
    [ColourCode] nvarchar(128) NULL,
    CONSTRAINT PK_ObservationOutcomes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Observations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Observations] (
    [Id] uniqueidentifier NOT NULL,
    [Date] datetime2(7) NOT NULL,
    [ObserveeId] uniqueidentifier NOT NULL,
    [ObserverId] uniqueidentifier NOT NULL,
    [OutcomeId] uniqueidentifier NOT NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_Observations PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ParentEveningAppointments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ParentEveningAppointments] (
    [Id] uniqueidentifier NOT NULL,
    [ParentEveningStaffMemberId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Start] datetime2(7) NOT NULL,
    [End] datetime2(7) NOT NULL,
    [HasTeacherAccepted] bit NOT NULL,
    [HasParentAccepted] bit NOT NULL,
    [HasAttended] bit NULL,
    CONSTRAINT PK_ParentEveningAppointments PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ParentEveningBreaks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ParentEveningBreaks] (
    [Id] uniqueidentifier NOT NULL,
    [ParentEveningStaffMemberId] uniqueidentifier NOT NULL,
    [Start] datetime2(7) NOT NULL,
    [End] datetime2(7) NOT NULL,
    CONSTRAINT PK_ParentEveningBreaks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ParentEveningGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ParentEveningGroups] (
    [Id] uniqueidentifier NOT NULL,
    [ParentEveningId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ParentEveningGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ParentEveningStaffMembers]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ParentEveningStaffMembers] (
    [Id] uniqueidentifier NOT NULL,
    [ParentEveningId] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [AvailableFrom] datetime2(7) NULL,
    [AvailableTo] datetime2(7) NULL,
    [AppointmentLength] int NOT NULL,
    [BreakLimit] int NOT NULL,
    CONSTRAINT PK_ParentEveningStaffMembers PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ParentEvenings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ParentEvenings] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Bookinpened] datetime2(7) NOT NULL,
    [BookingClosed] datetime2(7) NOT NULL,
    CONSTRAINT PK_ParentEvenings PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[People]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[People] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_People_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_People_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [DirectoryId] uniqueidentifier NOT NULL,
    [Title] nvarchar(128) NULL,
    [PreferredFirstName] nvarchar(256) NULL,
    [PreferredLastName] nvarchar(256) NULL,
    [FirstName] nvarchar(256) NOT NULL,
    [MiddleName] nvarchar(256) NULL,
    [LastName] nvarchar(256) NOT NULL,
    [PhotoId] uniqueidentifier NULL,
    [NhsNumber] nvarchar(10) NULL,
    [Gender] nvarchar(1) NOT NULL,
    [Dob] datetime2(7) NULL,
    [Deceased] datetime2(7) NULL,
    [EthnicityId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_People PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Permissions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [FriendlyName] nvarchar(256) NOT NULL,
    [Area] nvarchar(50) NOT NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[PersonConditions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PersonConditions] (
    [Id] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [MedicalConditionId] uniqueidentifier NOT NULL,
    [MedicationTaken] bit NOT NULL,
    [Medication] nvarchar(256) NULL,
    CONSTRAINT PK_PersonConditions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[PersonDietaryRequirements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PersonDietaryRequirements] (
    [Id] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [DietaryRequirementId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_PersonDietaryRequirements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[PhoneNumberTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PhoneNumberTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_PhoneNumberTypes_Active DEFAULT (1),
    CONSTRAINT PK_PhoneNumberTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[PhoneNumbers]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PhoneNumbers] (
    [Id] uniqueidentifier NOT NULL,
    [TypeId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NULL,
    [AgencyId] uniqueidentifier NULL,
    [Number] nvarchar(128) NOT NULL,
    [IsMain] bit NOT NULL,
    CONSTRAINT PK_PhoneNumbers PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Photos]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Photos] (
    [Id] uniqueidentifier NOT NULL,
    [Data] varbinary(max) NOT NULL,
    [PhotoDate] datetime2(7) NOT NULL,
    [MimeType] nvarchar(256) NOT NULL,
    CONSTRAINT PK_Photos PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ProductTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ProductTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ProductTypes_Active DEFAULT (1),
    CONSTRAINT PK_ProductTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Products] (
    [Id] uniqueidentifier NOT NULL,
    [ProductTypeId] uniqueidentifier NOT NULL,
    [VatRateId] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [ShowOnStore] bit NOT NULL,
    [OrderLimit] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[RegGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RegGroups] (
    [Id] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [YearGroupId] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NULL,
     CONSTRAINT PK_RegGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[RelationshipTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RelationshipTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_RelationshipTypes_Active DEFAULT (1),
    CONSTRAINT PK_RelationshipTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ReportCardEntries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReportCardEntries] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_ReportCardEntries_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_ReportCardEntries_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [ReportCardId] uniqueidentifier NOT NULL,
    [AttendanceWeekId] uniqueidentifier NOT NULL,
    [AttendancePeriodId] uniqueidentifier NOT NULL,
    [Comments] nvarchar(256) NULL,
    CONSTRAINT PK_ReportCardEntries PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ReportCardTargetEntries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReportCardTargetEntries] (
    [Id] uniqueidentifier NOT NULL,
    [EntryId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
    [IsCompleted] bit NOT NULL,
     CONSTRAINT PK_ReportCardTargetEntries PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ReportCardTargets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReportCardTargets] (
    [Id] uniqueidentifier NOT NULL,
    [ReportCardId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_ReportCardTargets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ReportCards]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReportCards] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [BehaviourTypeId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    [Comments] nvarchar(256) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT PK_ReportCards PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ResultSetReleases]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ResultSetReleases] (
    [Id] uniqueidentifier NOT NULL,
    [ResultSetId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [ReleasedAt] datetime2(7) NOT NULL,
    CONSTRAINT PK_ResultSetReleases PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[ResultSets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ResultSets] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ResultSets_Active DEFAULT (1),
    [Name] nvarchar(256) NOT NULL,
    [IsLocked] bit NOT NULL,
    CONSTRAINT PK_ResultSets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Results]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Results] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Results_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Results_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [ResultSetId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [AspectId] uniqueidentifier NOT NULL,
    [Date] datetime2(7) NOT NULL,
    [GradeId] uniqueidentifier NULL,
    [Mark] decimal(18,2) NULL,
    [Comment] nvarchar(1000) NULL,
    [ColourCode] nvarchar(256) NULL,
    [Note] nvarchar(256) NULL,
    CONSTRAINT PK_Results PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[RolePermissions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RolePermissions] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[RoomClosureReasons]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RoomClosureReasons] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_RoomClosureReasons_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_RoomClosureReasons PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[RoomClosures]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RoomClosures] (
    [Id] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NOT NULL,
    [ReasonId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_RoomClosures PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Rooms]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Rooms] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingFloorId] uniqueidentifier NULL,
    [Code] nvarchar(10) NULL,
    [Name] nvarchar(256) NOT NULL,
    [MaxGroupSize] int NOT NULL,
    [TelephoneNo] nvarchar(256) NULL,
    [IsExcludedFromCover] bit NOT NULL,
    CONSTRAINT PK_Rooms PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SchoolPhases]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SchoolPhases] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SchoolPhases_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_SchoolPhases PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SchoolTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SchoolTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SchoolTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_SchoolTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Schools]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Schools] (
    [Id] uniqueidentifier NOT NULL,
    [AgencyId] uniqueidentifier NOT NULL,
    [LocalAuthorityId] uniqueidentifier NULL,
    [EstablishmentNumber] int NOT NULL,
    [Urn] nvarchar(128) NOT NULL,
    [Uprn] nvarchar(128) NOT NULL,
    [SchoolPhaseId] uniqueidentifier NOT NULL,
    [SchoolTypeId] uniqueidentifier NOT NULL,
    [GovernanceTypeId] uniqueidentifier NOT NULL,
    [IntakeTypeId] uniqueidentifier NOT NULL,
    [HeadTeacherId] uniqueidentifier NULL,
    [IsLocal] bit NOT NULL,
    CONSTRAINT PK_Schools PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenEventTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenEventTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenEventTypes_Active DEFAULT (1),
    CONSTRAINT PK_SenEventTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenEvents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenEvents] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SenEventTypeId] uniqueidentifier NOT NULL,
    [Date] datetime2(7) NOT NULL,
    [Note] nvarchar(256) NOT NULL,
    CONSTRAINT PK_SenEvents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenProvisionTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenProvisionTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenProvisionTypes_Active DEFAULT (1),
    CONSTRAINT PK_SenProvisionTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenProvisions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenProvisions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SenProvisionTypeId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    [Note] nvarchar(256) NOT NULL,
    CONSTRAINT PK_SenProvisions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenReviewStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenReviewStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenReviewStatus_Active DEFAULT (1),
    CONSTRAINT PK_SenReviewStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenReviewTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenReviewTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenReviewTypes_Active DEFAULT (1),
    CONSTRAINT PK_SenReviewTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenReviews]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenReviews] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SenReviewTypeId] uniqueidentifier NOT NULL,
    [SenReviewStatusId] uniqueidentifier NOT NULL,
    [SencoId] uniqueidentifier NULL,
    [DiaryEventId] uniqueidentifier NOT NULL,
    [OutcomeSenStatusId] uniqueidentifier NULL,
    [Comments] nvarchar(256) NULL,
    CONSTRAINT PK_SenReviews PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenStatus_Active DEFAULT (1),
    [Code] nvarchar(1) NOT NULL,
    CONSTRAINT PK_SenStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SenTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SenTypes_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    CONSTRAINT PK_SenTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SessionExtraNames]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SessionExtraNames] (
    [Id] uniqueidentifier NOT NULL,
    [AttendanceWeekId] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_SessionExtraNames PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SessionPeriods]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SessionPeriods] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [PeriodId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_SessionPeriods PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SessionTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SessionTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SessionTypes_Active DEFAULT (1),
    [Code] nvarchar(256) NULL,
    [Length] int NOT NULL,
    CONSTRAINT PK_SessionTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Sessions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Sessions] (
    [Id] uniqueidentifier NOT NULL,
    [ClassId] uniqueidentifier NOT NULL,
    [TeacherId] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    CONSTRAINT PK_Sessions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StaffAbsenceTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffAbsenceTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_StaffAbsenceTypes_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    [IsAuthorised] bit NOT NULL,
    CONSTRAINT PK_StaffAbsenceTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StaffAbsences]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffAbsences] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [AbsenceTypeId] uniqueidentifier NOT NULL,
    [IllnessTypeId] uniqueidentifier NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NOT NULL,
    [IsConfidential] bit NOT NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_StaffAbsences PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StaffIllnessTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffIllnessTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_StaffIllnessTypes_Active DEFAULT (1),
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_StaffIllnessTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StaffMembers]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffMembers] (
    [Id] uniqueidentifier NOT NULL,
    [LineManagerId] uniqueidentifier NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [Code] nvarchar(128) NOT NULL,
    [BankName] nvarchar(50) NULL,
    [BankAccount] nvarchar(15) NULL,
    [BankSortCode] nvarchar(10) NULL,
    [NiNumber] nvarchar(9) NULL,
    [Qualifications] nvarchar(128) NULL,
    [IsTeachingStaff] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_StaffMembers PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StoreDiscounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StoreDiscounts] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NULL,
    [ProductTypeId] uniqueidentifier NULL,
    [DiscountId] uniqueidentifier NOT NULL,
    [IsAppliedToCart] bit NOT NULL,
    [ApplyTo] int NULL,
     CONSTRAINT PK_StoreDiscounts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentAchievements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentAchievements] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [AchievementId] uniqueidentifier NOT NULL,
    [OutcomeId] uniqueidentifier NULL,
    [Points] int NOT NULL,
     CONSTRAINT PK_StudentAchievements PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentAgentRelationships]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentAgentRelationships] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [AgentId] uniqueidentifier NOT NULL,
    [RelationshipTypeId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_StudentAgentRelationships PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentChargeDiscounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentChargeDiscounts] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ChargeDiscountId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_StudentChargeDiscounts PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentCharges]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentCharges] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ChargeId] uniqueidentifier NOT NULL,
    [ChargeBillingPeriodId] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NULL,
    CONSTRAINT PK_StudentCharges PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentContactRelationships]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentContactRelationships] (
    [Id] uniqueidentifier NOT NULL,
    [RelationshipTypeId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ContactId] uniqueidentifier NOT NULL,
    [HasCorrespondence] bit NOT NULL,
    [HasParentalResponsibility] bit NOT NULL,
    [HasPupilReport] bit NOT NULL,
    [HasCourtOrder] bit NOT NULL,
     CONSTRAINT PK_StudentContactRelationships PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentDetentions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentDetentions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [DetentionId] uniqueidentifier NOT NULL,
    [LinkedIncidentId] uniqueidentifier NULL,
    [HasAttended] bit NOT NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_StudentDetentions PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentGroupMemberships]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentGroupMemberships] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    CONSTRAINT PK_StudentGroupMemberships PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentGroupSupervisors]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentGroupSupervisors] (
    [Id] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [SupervisorId] uniqueidentifier NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    CONSTRAINT PK_StudentGroupSupervisors PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentGroups] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_StudentGroups_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    [PromoteToGroupId] uniqueidentifier NULL,
    [MainSupervisorId] uniqueidentifier NULL,
    [MaxMembers] int NULL,
    [Notes] nvarchar(256) NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_StudentGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudentIncidents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentIncidents] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [IncidentId] uniqueidentifier NOT NULL,
    [RoleTypeId] uniqueidentifier NOT NULL,
    [OutcomeId] uniqueidentifier NULL,
    [StatusId] uniqueidentifier NOT NULL,
    [Points] int NOT NULL,
     CONSTRAINT PK_StudentIncidents PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Students] (
    [Id] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [AdmissionNumber] int NOT NULL,
    [DateStarting] datetime2(7) NULL,
    [DateLeaving] datetime2(7) NULL,
    [FreeSchoolMeals] bit NOT NULL,
    [SenStatusId] uniqueidentifier NULL,
    [SenTypeId] uniqueidentifier NULL,
    [EnrolmentStatusId] uniqueidentifier NULL,
    [BoarderStatusId] uniqueidentifier NULL,
    [PupilPremium] bit NOT NULL,
    [Upn] nvarchar(13) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[StudyTopics]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudyTopics] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_StudyTopics_Active DEFAULT (1),
    [CourseId] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Order] int NOT NULL,
    CONSTRAINT PK_StudyTopics PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SubjectCodeSets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SubjectCodeSets] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SubjectCodeSets_Active DEFAULT (1),
    CONSTRAINT PK_SubjectCodeSets PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SubjectCodes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SubjectCodes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SubjectCodes_Active DEFAULT (1),
    [Code] nvarchar(256) NOT NULL,
    [SubjectCodeSetId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_SubjectCodes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SubjectStaffMemberRoles]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SubjectStaffMemberRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SubjectStaffMemberRoles_Active DEFAULT (1),
    [IsSubjectLeader] bit NOT NULL,
    CONSTRAINT PK_SubjectStaffMemberRoles PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SubjectStaffMembers]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SubjectStaffMembers] (
    [Id] uniqueidentifier NOT NULL,
    [SubjectId] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_SubjectStaffMembers PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Subjects]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Subjects] (
    [Id] uniqueidentifier NOT NULL,
    [SubjectCodeId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Code] nvarchar(5) NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT PK_Subjects PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[SystemSettings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SystemSettings] (
    [Id] uniqueidentifier NOT NULL,
    [SystemSetting] nvarchar(256) NOT NULL,
    [Setting] nvarchar(256) NOT NULL,
    CONSTRAINT PK_SystemSettings PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[TaskReminders]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TaskReminders] (
    [Id] uniqueidentifier NOT NULL,
    [TaskId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RemindTime] datetime2(7) NOT NULL,
    CONSTRAINT PK_TaskReminders PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[TaskTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TaskTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_TaskTypes_Active DEFAULT (1),
    [IsPersonal] bit NOT NULL,
    [ColourCode] nvarchar(256) NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_TaskTypes PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Tasks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Tasks] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(40) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Tasks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [TypeId] uniqueidentifier NOT NULL,
    [AssignedToId] uniqueidentifier NULL,
    [DueDate] datetime2(7) NULL,
    [CompletedDate] datetime2(7) NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [Completed] bit NOT NULL,
    [CanAssigneeEdit] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_Tasks PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[TrainingCertificateStatus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TrainingCertificateStatus] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_TrainingCertificateStatus_Active DEFAULT (1),
    [ColourCode] nvarchar(128) NULL,
    CONSTRAINT PK_TrainingCertificateStatus PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[TrainingCertificates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TrainingCertificates] (
    [Id] uniqueidentifier NOT NULL,
    [TrainingCourseId] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [TrainingCertificateStatusId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_TrainingCertificates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[TrainingCourses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TrainingCourses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_TrainingCourses_Active DEFAULT (1),
    [Code] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    CONSTRAINT PK_TrainingCourses PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[UserReminderSettings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[UserReminderSettings] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ReminderType] uniqueidentifier NOT NULL,
    [RemindBefore] time(7) NOT NULL,
    CONSTRAINT PK_UserReminderSettings PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Users] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL,
    [PersonId] uniqueidentifier NULL,
    [UserType] int NOT NULL,
    [IsEnabled] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[VatRates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[VatRates] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_VatRates_Active DEFAULT (1),
    [Value] decimal(18,2) NOT NULL,
    CONSTRAINT PK_VatRates PRIMARY KEY CLUSTERED ([Id])
    );
END


IF OBJECT_ID(N'[dbo].[YearGroups]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[YearGroups] (
    [Id] uniqueidentifier NOT NULL,
    [StudentGroupId] uniqueidentifier NOT NULL,
    [CurriculumYearGroupId] uniqueidentifier NOT NULL,
     CONSTRAINT PK_YearGroups PRIMARY KEY CLUSTERED ([Id])
    );
END


-- === FOREIGN KEYS ===

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AcademicTerms_AcademicYearId_AcademicYears' AND parent_object_id = OBJECT_ID(N'[dbo].[AcademicTerms]'))
BEGIN
ALTER TABLE [dbo].[AcademicTerms]
    ADD CONSTRAINT [FK_AcademicTerms_AcademicYearId_AcademicYears] FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AcademicTerms_AcademicYearId' AND object_id = OBJECT_ID(N'[dbo].[AcademicTerms]'))
BEGIN
CREATE INDEX [IX_AcademicTerms_AcademicYearId] ON [dbo].[AcademicTerms]([AcademicYearId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AccountTransactions_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[AccountTransactions]'))
BEGIN
ALTER TABLE [dbo].[AccountTransactions]
    ADD CONSTRAINT [FK_AccountTransactions_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AccountTransactions_StudentId' AND object_id = OBJECT_ID(N'[dbo].[AccountTransactions]'))
BEGIN
CREATE INDEX [IX_AccountTransactions_StudentId] ON [dbo].[AccountTransactions]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Achievements_AchievementTypeId_AchievementTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
ALTER TABLE [dbo].[Achievements]
    ADD CONSTRAINT [FK_Achievements_AchievementTypeId_AchievementTypes] FOREIGN KEY ([AchievementTypeId]) REFERENCES [dbo].[AchievementTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_AchievementTypeId' AND object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
CREATE INDEX [IX_Achievements_AchievementTypeId] ON [dbo].[Achievements]([AchievementTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Achievements_LocationId_Locations' AND parent_object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
ALTER TABLE [dbo].[Achievements]
    ADD CONSTRAINT [FK_Achievements_LocationId_Locations] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_LocationId' AND object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
CREATE INDEX [IX_Achievements_LocationId] ON [dbo].[Achievements]([LocationId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Achievements_AcademicYearId_AcademicYears' AND parent_object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
ALTER TABLE [dbo].[Achievements]
    ADD CONSTRAINT [FK_Achievements_AcademicYearId_AcademicYears] FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_AcademicYearId' AND object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
CREATE INDEX [IX_Achievements_AcademicYearId] ON [dbo].[Achievements]([AcademicYearId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Achievements_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
ALTER TABLE [dbo].[Achievements]
    ADD CONSTRAINT [FK_Achievements_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
CREATE INDEX [IX_Achievements_CreatedById] ON [dbo].[Achievements]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Achievements_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
ALTER TABLE [dbo].[Achievements]
    ADD CONSTRAINT [FK_Achievements_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Achievements]'))
BEGIN
CREATE INDEX [IX_Achievements_LastModifiedById] ON [dbo].[Achievements]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Activities_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[Activities]'))
BEGIN
ALTER TABLE [dbo].[Activities]
    ADD CONSTRAINT [FK_Activities_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Activities_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[Activities]'))
BEGIN
CREATE INDEX [IX_Activities_StudentGroupId] ON [dbo].[Activities]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressAgencies_AddressId_Addresses' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
ALTER TABLE [dbo].[AddressAgencies]
    ADD CONSTRAINT [FK_AddressAgencies_AddressId_Addresses] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Addresses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressAgencies_AddressId' AND object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
CREATE INDEX [IX_AddressAgencies_AddressId] ON [dbo].[AddressAgencies]([AddressId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressAgencies_AgencyId_Agencies' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
ALTER TABLE [dbo].[AddressAgencies]
    ADD CONSTRAINT [FK_AddressAgencies_AgencyId_Agencies] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressAgencies_AgencyId' AND object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
CREATE INDEX [IX_AddressAgencies_AgencyId] ON [dbo].[AddressAgencies]([AgencyId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressAgencies_AddressTypeId_AddressTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
ALTER TABLE [dbo].[AddressAgencies]
    ADD CONSTRAINT [FK_AddressAgencies_AddressTypeId_AddressTypes] FOREIGN KEY ([AddressTypeId]) REFERENCES [dbo].[AddressTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressAgencies_AddressTypeId' AND object_id = OBJECT_ID(N'[dbo].[AddressAgencies]'))
BEGIN
CREATE INDEX [IX_AddressAgencies_AddressTypeId] ON [dbo].[AddressAgencies]([AddressTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_AddressId_Addresses' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_AddressId_Addresses] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Addresses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressPeople_AddressId' AND object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
CREATE INDEX [IX_AddressPeople_AddressId] ON [dbo].[AddressPeople]([AddressId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressPeople_PersonId' AND object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
CREATE INDEX [IX_AddressPeople_PersonId] ON [dbo].[AddressPeople]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_AddressTypeId_AddressTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_AddressTypeId_AddressTypes] FOREIGN KEY ([AddressTypeId]) REFERENCES [dbo].[AddressTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressPeople_AddressTypeId' AND object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
CREATE INDEX [IX_AddressPeople_AddressTypeId] ON [dbo].[AddressPeople]([AddressTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Agencies_AgencyTypeId_AgencyTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Agencies]'))
BEGIN
ALTER TABLE [dbo].[Agencies]
    ADD CONSTRAINT [FK_Agencies_AgencyTypeId_AgencyTypes] FOREIGN KEY ([AgencyTypeId]) REFERENCES [dbo].[AgencyTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agencies_AgencyTypeId' AND object_id = OBJECT_ID(N'[dbo].[Agencies]'))
BEGIN
CREATE INDEX [IX_Agencies_AgencyTypeId] ON [dbo].[Agencies]([AgencyTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Agencies_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[Agencies]'))
BEGIN
ALTER TABLE [dbo].[Agencies]
    ADD CONSTRAINT [FK_Agencies_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agencies_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[Agencies]'))
BEGIN
CREATE INDEX [IX_Agencies_DirectoryId] ON [dbo].[Agencies]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Agents_AgencyId_Agencies' AND parent_object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
ALTER TABLE [dbo].[Agents]
    ADD CONSTRAINT [FK_Agents_AgencyId_Agencies] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agents_AgencyId' AND object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
CREATE INDEX [IX_Agents_AgencyId] ON [dbo].[Agents]([AgencyId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Agents_AgentTypeId_AgentTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
ALTER TABLE [dbo].[Agents]
    ADD CONSTRAINT [FK_Agents_AgentTypeId_AgentTypes] FOREIGN KEY ([AgentTypeId]) REFERENCES [dbo].[AgentTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agents_AgentTypeId' AND object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
CREATE INDEX [IX_Agents_AgentTypeId] ON [dbo].[Agents]([AgentTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Agents_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
ALTER TABLE [dbo].[Agents]
    ADD CONSTRAINT [FK_Agents_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agents_PersonId' AND object_id = OBJECT_ID(N'[dbo].[Agents]'))
BEGIN
CREATE INDEX [IX_Agents_PersonId] ON [dbo].[Agents]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aspects_TypeId_AspectTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Aspects]'))
BEGIN
ALTER TABLE [dbo].[Aspects]
    ADD CONSTRAINT [FK_Aspects_TypeId_AspectTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[AspectTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Aspects_TypeId' AND object_id = OBJECT_ID(N'[dbo].[Aspects]'))
BEGIN
CREATE INDEX [IX_Aspects_TypeId] ON [dbo].[Aspects]([TypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aspects_GradeSetId_GradeSets' AND parent_object_id = OBJECT_ID(N'[dbo].[Aspects]'))
BEGIN
ALTER TABLE [dbo].[Aspects]
    ADD CONSTRAINT [FK_Aspects_GradeSetId_GradeSets] FOREIGN KEY ([GradeSetId]) REFERENCES [dbo].[GradeSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Aspects_GradeSetId' AND object_id = OBJECT_ID(N'[dbo].[Aspects]'))
BEGIN
CREATE INDEX [IX_Aspects_GradeSetId] ON [dbo].[Aspects]([GradeSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceCodes_AttendanceCodeTypeId_AttendanceCodeTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceCodes]'))
BEGIN
ALTER TABLE [dbo].[AttendanceCodes]
    ADD CONSTRAINT [FK_AttendanceCodes_AttendanceCodeTypeId_AttendanceCodeTypes] FOREIGN KEY ([AttendanceCodeTypeId]) REFERENCES [dbo].[AttendanceCodeTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceCodes_AttendanceCodeTypeId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceCodes]'))
BEGIN
CREATE INDEX [IX_AttendanceCodes_AttendanceCodeTypeId] ON [dbo].[AttendanceCodes]([AttendanceCodeTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_AttendanceCodeId_AttendanceCodes' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_AttendanceCodeId_AttendanceCodes] FOREIGN KEY ([AttendanceCodeId]) REFERENCES [dbo].[AttendanceCodes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_AttendanceCodeId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_AttendanceCodeId] ON [dbo].[AttendanceMarks]([AttendanceCodeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_AttendancePeriodId_AttendancePeriods' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_AttendancePeriodId_AttendancePeriods] FOREIGN KEY ([AttendancePeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_AttendancePeriodId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_AttendancePeriodId] ON [dbo].[AttendanceMarks]([AttendancePeriodId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_StudentId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_StudentId] ON [dbo].[AttendanceMarks]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_AttendanceWeekId_AttendanceWeeks' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_AttendanceWeekId_AttendanceWeeks] FOREIGN KEY ([AttendanceWeekId]) REFERENCES [dbo].[AttendanceWeeks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_AttendanceWeekId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_AttendanceWeekId] ON [dbo].[AttendanceMarks]([AttendanceWeekId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_CreatedById] ON [dbo].[AttendanceMarks]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceMarks_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceMarks]
    ADD CONSTRAINT [FK_AttendanceMarks_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceMarks_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[AttendanceMarks]'))
BEGIN
CREATE INDEX [IX_AttendanceMarks_LastModifiedById] ON [dbo].[AttendanceMarks]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendancePeriods_WeekPatternId_AttendanceWeekPatterns' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendancePeriods]'))
BEGIN
ALTER TABLE [dbo].[AttendancePeriods]
    ADD CONSTRAINT [FK_AttendancePeriods_WeekPatternId_AttendanceWeekPatterns] FOREIGN KEY ([WeekPatternId]) REFERENCES [dbo].[AttendanceWeekPatterns]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendancePeriods_WeekPatternId' AND object_id = OBJECT_ID(N'[dbo].[AttendancePeriods]'))
BEGIN
CREATE INDEX [IX_AttendancePeriods_WeekPatternId] ON [dbo].[AttendancePeriods]([WeekPatternId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceWeeks_AcademicTermId_AcademicTerms' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceWeeks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceWeeks]
    ADD CONSTRAINT [FK_AttendanceWeeks_AcademicTermId_AcademicTerms] FOREIGN KEY ([AcademicTermId]) REFERENCES [dbo].[AcademicTerms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceWeeks_AcademicTermId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceWeeks]'))
BEGIN
CREATE INDEX [IX_AttendanceWeeks_AcademicTermId] ON [dbo].[AttendanceWeeks]([AcademicTermId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AttendanceWeeks_WeekPatternId_AttendanceWeekPatterns' AND parent_object_id = OBJECT_ID(N'[dbo].[AttendanceWeeks]'))
BEGIN
ALTER TABLE [dbo].[AttendanceWeeks]
    ADD CONSTRAINT [FK_AttendanceWeeks_WeekPatternId_AttendanceWeekPatterns] FOREIGN KEY ([WeekPatternId]) REFERENCES [dbo].[AttendanceWeekPatterns]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceWeeks_WeekPatternId' AND object_id = OBJECT_ID(N'[dbo].[AttendanceWeeks]'))
BEGIN
CREATE INDEX [IX_AttendanceWeeks_WeekPatternId] ON [dbo].[AttendanceWeeks]([WeekPatternId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BasketItems_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[BasketItems]'))
BEGIN
ALTER TABLE [dbo].[BasketItems]
    ADD CONSTRAINT [FK_BasketItems_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BasketItems_StudentId' AND object_id = OBJECT_ID(N'[dbo].[BasketItems]'))
BEGIN
CREATE INDEX [IX_BasketItems_StudentId] ON [dbo].[BasketItems]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BasketItems_ProductId_Products' AND parent_object_id = OBJECT_ID(N'[dbo].[BasketItems]'))
BEGIN
ALTER TABLE [dbo].[BasketItems]
    ADD CONSTRAINT [FK_BasketItems_ProductId_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BasketItems_ProductId' AND object_id = OBJECT_ID(N'[dbo].[BasketItems]'))
BEGIN
CREATE INDEX [IX_BasketItems_ProductId] ON [dbo].[BasketItems]([ProductId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bills_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
ALTER TABLE [dbo].[Bills]
    ADD CONSTRAINT [FK_Bills_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bills_StudentId' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
CREATE INDEX [IX_Bills_StudentId] ON [dbo].[Bills]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bills_ChargeBillingPeriodId_ChargeBillingPeriods' AND parent_object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
ALTER TABLE [dbo].[Bills]
    ADD CONSTRAINT [FK_Bills_ChargeBillingPeriodId_ChargeBillingPeriods] FOREIGN KEY ([ChargeBillingPeriodId]) REFERENCES [dbo].[ChargeBillingPeriods]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bills_ChargeBillingPeriodId' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
CREATE INDEX [IX_Bills_ChargeBillingPeriodId] ON [dbo].[Bills]([ChargeBillingPeriodId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bills_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
ALTER TABLE [dbo].[Bills]
    ADD CONSTRAINT [FK_Bills_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bills_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
CREATE INDEX [IX_Bills_CreatedById] ON [dbo].[Bills]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bills_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
ALTER TABLE [dbo].[Bills]
    ADD CONSTRAINT [FK_Bills_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bills_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
BEGIN
CREATE INDEX [IX_Bills_LastModifiedById] ON [dbo].[Bills]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillAccountTransactions_BillId_Bills' AND parent_object_id = OBJECT_ID(N'[dbo].[BillAccountTransactions]'))
BEGIN
ALTER TABLE [dbo].[BillAccountTransactions]
    ADD CONSTRAINT [FK_BillAccountTransactions_BillId_Bills] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillAccountTransactions_BillId' AND object_id = OBJECT_ID(N'[dbo].[BillAccountTransactions]'))
BEGIN
CREATE INDEX [IX_BillAccountTransactions_BillId] ON [dbo].[BillAccountTransactions]([BillId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillAccountTransactions_AccountTransactionId_AccountTransactions' AND parent_object_id = OBJECT_ID(N'[dbo].[BillAccountTransactions]'))
BEGIN
ALTER TABLE [dbo].[BillAccountTransactions]
    ADD CONSTRAINT [FK_BillAccountTransactions_AccountTransactionId_AccountTransactions] FOREIGN KEY ([AccountTransactionId]) REFERENCES [dbo].[AccountTransactions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillAccountTransactions_AccountTransactionId' AND object_id = OBJECT_ID(N'[dbo].[BillAccountTransactions]'))
BEGIN
CREATE INDEX [IX_BillAccountTransactions_AccountTransactionId] ON [dbo].[BillAccountTransactions]([AccountTransactionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillDiscounts_BillId_Bills' AND parent_object_id = OBJECT_ID(N'[dbo].[BillDiscounts]'))
BEGIN
ALTER TABLE [dbo].[BillDiscounts]
    ADD CONSTRAINT [FK_BillDiscounts_BillId_Bills] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillDiscounts_BillId' AND object_id = OBJECT_ID(N'[dbo].[BillDiscounts]'))
BEGIN
CREATE INDEX [IX_BillDiscounts_BillId] ON [dbo].[BillDiscounts]([BillId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillDiscounts_DiscountId_Discounts' AND parent_object_id = OBJECT_ID(N'[dbo].[BillDiscounts]'))
BEGIN
ALTER TABLE [dbo].[BillDiscounts]
    ADD CONSTRAINT [FK_BillDiscounts_DiscountId_Discounts] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discounts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillDiscounts_DiscountId' AND object_id = OBJECT_ID(N'[dbo].[BillDiscounts]'))
BEGIN
CREATE INDEX [IX_BillDiscounts_DiscountId] ON [dbo].[BillDiscounts]([DiscountId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillItems_BillId_Bills' AND parent_object_id = OBJECT_ID(N'[dbo].[BillItems]'))
BEGIN
ALTER TABLE [dbo].[BillItems]
    ADD CONSTRAINT [FK_BillItems_BillId_Bills] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillItems_BillId' AND object_id = OBJECT_ID(N'[dbo].[BillItems]'))
BEGIN
CREATE INDEX [IX_BillItems_BillId] ON [dbo].[BillItems]([BillId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillItems_ProductId_Products' AND parent_object_id = OBJECT_ID(N'[dbo].[BillItems]'))
BEGIN
ALTER TABLE [dbo].[BillItems]
    ADD CONSTRAINT [FK_BillItems_ProductId_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillItems_ProductId' AND object_id = OBJECT_ID(N'[dbo].[BillItems]'))
BEGIN
CREATE INDEX [IX_BillItems_ProductId] ON [dbo].[BillItems]([ProductId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillCharges_BillId_Bills' AND parent_object_id = OBJECT_ID(N'[dbo].[BillCharges]'))
BEGIN
ALTER TABLE [dbo].[BillCharges]
    ADD CONSTRAINT [FK_BillCharges_BillId_Bills] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillCharges_BillId' AND object_id = OBJECT_ID(N'[dbo].[BillCharges]'))
BEGIN
CREATE INDEX [IX_BillCharges_BillId] ON [dbo].[BillCharges]([BillId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BillCharges_StudentChargeId_StudentCharges' AND parent_object_id = OBJECT_ID(N'[dbo].[BillCharges]'))
BEGIN
ALTER TABLE [dbo].[BillCharges]
    ADD CONSTRAINT [FK_BillCharges_StudentChargeId_StudentCharges] FOREIGN KEY ([StudentChargeId]) REFERENCES [dbo].[StudentCharges]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillCharges_StudentChargeId' AND object_id = OBJECT_ID(N'[dbo].[BillCharges]'))
BEGIN
CREATE INDEX [IX_BillCharges_StudentChargeId] ON [dbo].[BillCharges]([StudentChargeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BuildingFloors_BuildingId_Buildings' AND parent_object_id = OBJECT_ID(N'[dbo].[BuildingFloors]'))
BEGIN
ALTER TABLE [dbo].[BuildingFloors]
    ADD CONSTRAINT [FK_BuildingFloors_BuildingId_Buildings] FOREIGN KEY ([BuildingId]) REFERENCES [dbo].[Buildings]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BuildingFloors_BuildingId' AND object_id = OBJECT_ID(N'[dbo].[BuildingFloors]'))
BEGIN
CREATE INDEX [IX_BuildingFloors_BuildingId] ON [dbo].[BuildingFloors]([BuildingId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bulletins_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
ALTER TABLE [dbo].[Bulletins]
    ADD CONSTRAINT [FK_Bulletins_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bulletins_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
CREATE INDEX [IX_Bulletins_DirectoryId] ON [dbo].[Bulletins]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bulletins_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
ALTER TABLE [dbo].[Bulletins]
    ADD CONSTRAINT [FK_Bulletins_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bulletins_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
CREATE INDEX [IX_Bulletins_CreatedById] ON [dbo].[Bulletins]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bulletins_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
ALTER TABLE [dbo].[Bulletins]
    ADD CONSTRAINT [FK_Bulletins_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bulletins_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Bulletins]'))
BEGIN
CREATE INDEX [IX_Bulletins_LastModifiedById] ON [dbo].[Bulletins]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Charges_VatRateId_VatRates' AND parent_object_id = OBJECT_ID(N'[dbo].[Charges]'))
BEGIN
ALTER TABLE [dbo].[Charges]
    ADD CONSTRAINT [FK_Charges_VatRateId_VatRates] FOREIGN KEY ([VatRateId]) REFERENCES [dbo].[VatRates]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Charges_VatRateId' AND object_id = OBJECT_ID(N'[dbo].[Charges]'))
BEGIN
CREATE INDEX [IX_Charges_VatRateId] ON [dbo].[Charges]([VatRateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ChargeDiscounts_ChargeId_Charges' AND parent_object_id = OBJECT_ID(N'[dbo].[ChargeDiscounts]'))
BEGIN
ALTER TABLE [dbo].[ChargeDiscounts]
    ADD CONSTRAINT [FK_ChargeDiscounts_ChargeId_Charges] FOREIGN KEY ([ChargeId]) REFERENCES [dbo].[Charges]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChargeDiscounts_ChargeId' AND object_id = OBJECT_ID(N'[dbo].[ChargeDiscounts]'))
BEGIN
CREATE INDEX [IX_ChargeDiscounts_ChargeId] ON [dbo].[ChargeDiscounts]([ChargeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ChargeDiscounts_DiscountId_Discounts' AND parent_object_id = OBJECT_ID(N'[dbo].[ChargeDiscounts]'))
BEGIN
ALTER TABLE [dbo].[ChargeDiscounts]
    ADD CONSTRAINT [FK_ChargeDiscounts_DiscountId_Discounts] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discounts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChargeDiscounts_DiscountId' AND object_id = OBJECT_ID(N'[dbo].[ChargeDiscounts]'))
BEGIN
CREATE INDEX [IX_ChargeDiscounts_DiscountId] ON [dbo].[ChargeDiscounts]([DiscountId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Classes_CourseId_Courses' AND parent_object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
ALTER TABLE [dbo].[Classes]
    ADD CONSTRAINT [FK_Classes_CourseId_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Classes_CourseId' AND object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
CREATE INDEX [IX_Classes_CourseId] ON [dbo].[Classes]([CourseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Classes_CurriculumGroupId_CurriculumGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
ALTER TABLE [dbo].[Classes]
    ADD CONSTRAINT [FK_Classes_CurriculumGroupId_CurriculumGroups] FOREIGN KEY ([CurriculumGroupId]) REFERENCES [dbo].[CurriculumGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Classes_CurriculumGroupId' AND object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
CREATE INDEX [IX_Classes_CurriculumGroupId] ON [dbo].[Classes]([CurriculumGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Classes_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
ALTER TABLE [dbo].[Classes]
    ADD CONSTRAINT [FK_Classes_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Classes_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[Classes]'))
BEGIN
CREATE INDEX [IX_Classes_DirectoryId] ON [dbo].[Classes]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Comments_CommentTypeId_CommentTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Comments]'))
BEGIN
ALTER TABLE [dbo].[Comments]
    ADD CONSTRAINT [FK_Comments_CommentTypeId_CommentTypes] FOREIGN KEY ([CommentTypeId]) REFERENCES [dbo].[CommentTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Comments_CommentTypeId' AND object_id = OBJECT_ID(N'[dbo].[Comments]'))
BEGIN
CREATE INDEX [IX_Comments_CommentTypeId] ON [dbo].[Comments]([CommentTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Comments_CommentBankSectionId_CommentBankSections' AND parent_object_id = OBJECT_ID(N'[dbo].[Comments]'))
BEGIN
ALTER TABLE [dbo].[Comments]
    ADD CONSTRAINT [FK_Comments_CommentBankSectionId_CommentBankSections] FOREIGN KEY ([CommentBankSectionId]) REFERENCES [dbo].[CommentBankSections]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Comments_CommentBankSectionId' AND object_id = OBJECT_ID(N'[dbo].[Comments]'))
BEGIN
CREATE INDEX [IX_Comments_CommentBankSectionId] ON [dbo].[Comments]([CommentBankSectionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CommentBankAreas_CommentBankId_CommentBanks' AND parent_object_id = OBJECT_ID(N'[dbo].[CommentBankAreas]'))
BEGIN
ALTER TABLE [dbo].[CommentBankAreas]
    ADD CONSTRAINT [FK_CommentBankAreas_CommentBankId_CommentBanks] FOREIGN KEY ([CommentBankId]) REFERENCES [dbo].[CommentBanks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommentBankAreas_CommentBankId' AND object_id = OBJECT_ID(N'[dbo].[CommentBankAreas]'))
BEGIN
CREATE INDEX [IX_CommentBankAreas_CommentBankId] ON [dbo].[CommentBankAreas]([CommentBankId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CommentBankAreas_CourseId_Courses' AND parent_object_id = OBJECT_ID(N'[dbo].[CommentBankAreas]'))
BEGIN
ALTER TABLE [dbo].[CommentBankAreas]
    ADD CONSTRAINT [FK_CommentBankAreas_CourseId_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommentBankAreas_CourseId' AND object_id = OBJECT_ID(N'[dbo].[CommentBankAreas]'))
BEGIN
CREATE INDEX [IX_CommentBankAreas_CourseId] ON [dbo].[CommentBankAreas]([CourseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CommentBankSections_CommentBankAreaId_CommentBankAreas' AND parent_object_id = OBJECT_ID(N'[dbo].[CommentBankSections]'))
BEGIN
ALTER TABLE [dbo].[CommentBankSections]
    ADD CONSTRAINT [FK_CommentBankSections_CommentBankAreaId_CommentBankAreas] FOREIGN KEY ([CommentBankAreaId]) REFERENCES [dbo].[CommentBankAreas]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommentBankSections_CommentBankAreaId' AND object_id = OBJECT_ID(N'[dbo].[CommentBankSections]'))
BEGIN
CREATE INDEX [IX_CommentBankSections_CommentBankAreaId] ON [dbo].[CommentBankSections]([CommentBankAreaId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CommunicationLogs_ContactId_Contacts' AND parent_object_id = OBJECT_ID(N'[dbo].[CommunicationLogs]'))
BEGIN
ALTER TABLE [dbo].[CommunicationLogs]
    ADD CONSTRAINT [FK_CommunicationLogs_ContactId_Contacts] FOREIGN KEY ([ContactId]) REFERENCES [dbo].[Contacts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationLogs_ContactId' AND object_id = OBJECT_ID(N'[dbo].[CommunicationLogs]'))
BEGIN
CREATE INDEX [IX_CommunicationLogs_ContactId] ON [dbo].[CommunicationLogs]([ContactId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CommunicationLogs_CommunicationTypeId_CommunicationTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[CommunicationLogs]'))
BEGIN
ALTER TABLE [dbo].[CommunicationLogs]
    ADD CONSTRAINT [FK_CommunicationLogs_CommunicationTypeId_CommunicationTypes] FOREIGN KEY ([CommunicationTypeId]) REFERENCES [dbo].[CommunicationTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationLogs_CommunicationTypeId' AND object_id = OBJECT_ID(N'[dbo].[CommunicationLogs]'))
BEGIN
CREATE INDEX [IX_CommunicationLogs_CommunicationTypeId] ON [dbo].[CommunicationLogs]([CommunicationTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Contacts_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Contacts]'))
BEGIN
ALTER TABLE [dbo].[Contacts]
    ADD CONSTRAINT [FK_Contacts_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contacts_PersonId' AND object_id = OBJECT_ID(N'[dbo].[Contacts]'))
BEGIN
CREATE INDEX [IX_Contacts_PersonId] ON [dbo].[Contacts]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Courses_SubjectId_Subjects' AND parent_object_id = OBJECT_ID(N'[dbo].[Courses]'))
BEGIN
ALTER TABLE [dbo].[Courses]
    ADD CONSTRAINT [FK_Courses_SubjectId_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Courses_SubjectId' AND object_id = OBJECT_ID(N'[dbo].[Courses]'))
BEGIN
CREATE INDEX [IX_Courses_SubjectId] ON [dbo].[Courses]([SubjectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CoverArrangements_WeekId_AttendanceWeeks' AND parent_object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
ALTER TABLE [dbo].[CoverArrangements]
    ADD CONSTRAINT [FK_CoverArrangements_WeekId_AttendanceWeeks] FOREIGN KEY ([WeekId]) REFERENCES [dbo].[AttendanceWeeks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoverArrangements_WeekId' AND object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
CREATE INDEX [IX_CoverArrangements_WeekId] ON [dbo].[CoverArrangements]([WeekId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CoverArrangements_SessionId_Sessions' AND parent_object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
ALTER TABLE [dbo].[CoverArrangements]
    ADD CONSTRAINT [FK_CoverArrangements_SessionId_Sessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[Sessions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoverArrangements_SessionId' AND object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
CREATE INDEX [IX_CoverArrangements_SessionId] ON [dbo].[CoverArrangements]([SessionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CoverArrangements_TeacherId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
ALTER TABLE [dbo].[CoverArrangements]
    ADD CONSTRAINT [FK_CoverArrangements_TeacherId_StaffMembers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoverArrangements_TeacherId' AND object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
CREATE INDEX [IX_CoverArrangements_TeacherId] ON [dbo].[CoverArrangements]([TeacherId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CoverArrangements_RoomId_Rooms' AND parent_object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
ALTER TABLE [dbo].[CoverArrangements]
    ADD CONSTRAINT [FK_CoverArrangements_RoomId_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoverArrangements_RoomId' AND object_id = OBJECT_ID(N'[dbo].[CoverArrangements]'))
BEGIN
CREATE INDEX [IX_CoverArrangements_RoomId] ON [dbo].[CoverArrangements]([RoomId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumBands_AcademicYearId_AcademicYears' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
ALTER TABLE [dbo].[CurriculumBands]
    ADD CONSTRAINT [FK_CurriculumBands_AcademicYearId_AcademicYears] FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumBands_AcademicYearId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
CREATE INDEX [IX_CurriculumBands_AcademicYearId] ON [dbo].[CurriculumBands]([AcademicYearId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumBands_CurriculumYearGroupId_CurriculumYearGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
ALTER TABLE [dbo].[CurriculumBands]
    ADD CONSTRAINT [FK_CurriculumBands_CurriculumYearGroupId_CurriculumYearGroups] FOREIGN KEY ([CurriculumYearGroupId]) REFERENCES [dbo].[CurriculumYearGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumBands_CurriculumYearGroupId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
CREATE INDEX [IX_CurriculumBands_CurriculumYearGroupId] ON [dbo].[CurriculumBands]([CurriculumYearGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumBands_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
ALTER TABLE [dbo].[CurriculumBands]
    ADD CONSTRAINT [FK_CurriculumBands_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumBands_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumBands]'))
BEGIN
CREATE INDEX [IX_CurriculumBands_StudentGroupId] ON [dbo].[CurriculumBands]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumBandBlockAssignments_BlockId_CurriculumBlocks' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumBandBlockAssignments]'))
BEGIN
ALTER TABLE [dbo].[CurriculumBandBlockAssignments]
    ADD CONSTRAINT [FK_CurriculumBandBlockAssignments_BlockId_CurriculumBlocks] FOREIGN KEY ([BlockId]) REFERENCES [dbo].[CurriculumBlocks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumBandBlockAssignments_BlockId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumBandBlockAssignments]'))
BEGIN
CREATE INDEX [IX_CurriculumBandBlockAssignments_BlockId] ON [dbo].[CurriculumBandBlockAssignments]([BlockId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumBandBlockAssignments_BandId_CurriculumBands' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumBandBlockAssignments]'))
BEGIN
ALTER TABLE [dbo].[CurriculumBandBlockAssignments]
    ADD CONSTRAINT [FK_CurriculumBandBlockAssignments_BandId_CurriculumBands] FOREIGN KEY ([BandId]) REFERENCES [dbo].[CurriculumBands]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumBandBlockAssignments_BandId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumBandBlockAssignments]'))
BEGIN
CREATE INDEX [IX_CurriculumBandBlockAssignments_BandId] ON [dbo].[CurriculumBandBlockAssignments]([BandId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumGroups_BlockId_CurriculumBlocks' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumGroups]'))
BEGIN
ALTER TABLE [dbo].[CurriculumGroups]
    ADD CONSTRAINT [FK_CurriculumGroups_BlockId_CurriculumBlocks] FOREIGN KEY ([BlockId]) REFERENCES [dbo].[CurriculumBlocks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumGroups_BlockId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumGroups]'))
BEGIN
CREATE INDEX [IX_CurriculumGroups_BlockId] ON [dbo].[CurriculumGroups]([BlockId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumGroups_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumGroups]'))
BEGIN
ALTER TABLE [dbo].[CurriculumGroups]
    ADD CONSTRAINT [FK_CurriculumGroups_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumGroups_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumGroups]'))
BEGIN
CREATE INDEX [IX_CurriculumGroups_StudentGroupId] ON [dbo].[CurriculumGroups]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumGroupSessions_CurriculumGroupId_CurriculumGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
ALTER TABLE [dbo].[CurriculumGroupSessions]
    ADD CONSTRAINT [FK_CurriculumGroupSessions_CurriculumGroupId_CurriculumGroups] FOREIGN KEY ([CurriculumGroupId]) REFERENCES [dbo].[CurriculumGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumGroupSessions_CurriculumGroupId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
CREATE INDEX [IX_CurriculumGroupSessions_CurriculumGroupId] ON [dbo].[CurriculumGroupSessions]([CurriculumGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumGroupSessions_SubjectId_Subjects' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
ALTER TABLE [dbo].[CurriculumGroupSessions]
    ADD CONSTRAINT [FK_CurriculumGroupSessions_SubjectId_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumGroupSessions_SubjectId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
CREATE INDEX [IX_CurriculumGroupSessions_SubjectId] ON [dbo].[CurriculumGroupSessions]([SubjectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CurriculumGroupSessions_SessionTypeId_SessionTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
ALTER TABLE [dbo].[CurriculumGroupSessions]
    ADD CONSTRAINT [FK_CurriculumGroupSessions_SessionTypeId_SessionTypes] FOREIGN KEY ([SessionTypeId]) REFERENCES [dbo].[SessionTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumGroupSessions_SessionTypeId' AND object_id = OBJECT_ID(N'[dbo].[CurriculumGroupSessions]'))
BEGIN
CREATE INDEX [IX_CurriculumGroupSessions_SessionTypeId] ON [dbo].[CurriculumGroupSessions]([SessionTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detentions_DetentionTypeId_DetentionTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
ALTER TABLE [dbo].[Detentions]
    ADD CONSTRAINT [FK_Detentions_DetentionTypeId_DetentionTypes] FOREIGN KEY ([DetentionTypeId]) REFERENCES [dbo].[DetentionTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Detentions_DetentionTypeId' AND object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
CREATE INDEX [IX_Detentions_DetentionTypeId] ON [dbo].[Detentions]([DetentionTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detentions_EventId_DiaryEvents' AND parent_object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
ALTER TABLE [dbo].[Detentions]
    ADD CONSTRAINT [FK_Detentions_EventId_DiaryEvents] FOREIGN KEY ([EventId]) REFERENCES [dbo].[DiaryEvents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Detentions_EventId' AND object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
CREATE INDEX [IX_Detentions_EventId] ON [dbo].[Detentions]([EventId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detentions_SupervisorId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
ALTER TABLE [dbo].[Detentions]
    ADD CONSTRAINT [FK_Detentions_SupervisorId_StaffMembers] FOREIGN KEY ([SupervisorId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Detentions_SupervisorId' AND object_id = OBJECT_ID(N'[dbo].[Detentions]'))
BEGIN
CREATE INDEX [IX_Detentions_SupervisorId] ON [dbo].[Detentions]([SupervisorId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEvents_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEvents]'))
BEGIN
ALTER TABLE [dbo].[DiaryEvents]
    ADD CONSTRAINT [FK_DiaryEvents_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEvents_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[DiaryEvents]'))
BEGIN
CREATE INDEX [IX_DiaryEvents_CreatedById] ON [dbo].[DiaryEvents]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEvents_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEvents]'))
BEGIN
ALTER TABLE [dbo].[DiaryEvents]
    ADD CONSTRAINT [FK_DiaryEvents_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEvents_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[DiaryEvents]'))
BEGIN
CREATE INDEX [IX_DiaryEvents_LastModifiedById] ON [dbo].[DiaryEvents]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEventAttendees_EventId_DiaryEvents' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
ALTER TABLE [dbo].[DiaryEventAttendees]
    ADD CONSTRAINT [FK_DiaryEventAttendees_EventId_DiaryEvents] FOREIGN KEY ([EventId]) REFERENCES [dbo].[DiaryEvents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEventAttendees_EventId' AND object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
CREATE INDEX [IX_DiaryEventAttendees_EventId] ON [dbo].[DiaryEventAttendees]([EventId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEventAttendees_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
ALTER TABLE [dbo].[DiaryEventAttendees]
    ADD CONSTRAINT [FK_DiaryEventAttendees_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEventAttendees_PersonId' AND object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
CREATE INDEX [IX_DiaryEventAttendees_PersonId] ON [dbo].[DiaryEventAttendees]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEventAttendees_ResponseId_DiaryEventAttendeeResponses' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
ALTER TABLE [dbo].[DiaryEventAttendees]
    ADD CONSTRAINT [FK_DiaryEventAttendees_ResponseId_DiaryEventAttendeeResponses] FOREIGN KEY ([ResponseId]) REFERENCES [dbo].[DiaryEventAttendeeResponses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEventAttendees_ResponseId' AND object_id = OBJECT_ID(N'[dbo].[DiaryEventAttendees]'))
BEGIN
CREATE INDEX [IX_DiaryEventAttendees_ResponseId] ON [dbo].[DiaryEventAttendees]([ResponseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEventTemplates_DiaryEventTypeId_DiaryEventTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[DiaryEventTemplates]'))
BEGIN
ALTER TABLE [dbo].[DiaryEventTemplates]
    ADD CONSTRAINT [FK_DiaryEventTemplates_DiaryEventTypeId_DiaryEventTypes] FOREIGN KEY ([DiaryEventTypeId]) REFERENCES [dbo].[DiaryEventTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiaryEventTemplates_DiaryEventTypeId' AND object_id = OBJECT_ID(N'[dbo].[DiaryEventTemplates]'))
BEGIN
CREATE INDEX [IX_DiaryEventTemplates_DiaryEventTypeId] ON [dbo].[DiaryEventTemplates]([DiaryEventTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Directories_ParentId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[Directories]'))
BEGIN
ALTER TABLE [dbo].[Directories]
    ADD CONSTRAINT [FK_Directories_ParentId_Directories] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Directories_ParentId' AND object_id = OBJECT_ID(N'[dbo].[Directories]'))
BEGIN
CREATE INDEX [IX_Directories_ParentId] ON [dbo].[Directories]([ParentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Documents_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
ALTER TABLE [dbo].[Documents]
    ADD CONSTRAINT [FK_Documents_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
CREATE INDEX [IX_Documents_DirectoryId] ON [dbo].[Documents]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Documents_TypeId_DocumentTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
ALTER TABLE [dbo].[Documents]
    ADD CONSTRAINT [FK_Documents_TypeId_DocumentTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[DocumentTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_TypeId' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
CREATE INDEX [IX_Documents_TypeId] ON [dbo].[Documents]([TypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Documents_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
ALTER TABLE [dbo].[Documents]
    ADD CONSTRAINT [FK_Documents_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
CREATE INDEX [IX_Documents_CreatedById] ON [dbo].[Documents]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Documents_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
ALTER TABLE [dbo].[Documents]
    ADD CONSTRAINT [FK_Documents_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
BEGIN
CREATE INDEX [IX_Documents_LastModifiedById] ON [dbo].[Documents]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_AgencyId_Agencies' AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_AgencyId_Agencies] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAddresses_AgencyId' AND object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
CREATE INDEX [IX_EmailAddresses_AgencyId] ON [dbo].[EmailAddresses]([AgencyId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAddresses_PersonId' AND object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
CREATE INDEX [IX_EmailAddresses_PersonId] ON [dbo].[EmailAddresses]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_TypeId_EmailAddressTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_TypeId_EmailAddressTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[EmailAddressTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAddresses_TypeId' AND object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
CREATE INDEX [IX_EmailAddresses_TypeId] ON [dbo].[EmailAddresses]([TypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAssessments_ExamBoardId_ExamBoards' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAssessments]'))
BEGIN
ALTER TABLE [dbo].[ExamAssessments]
    ADD CONSTRAINT [FK_ExamAssessments_ExamBoardId_ExamBoards] FOREIGN KEY ([ExamBoardId]) REFERENCES [dbo].[ExamBoards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAssessments_ExamBoardId' AND object_id = OBJECT_ID(N'[dbo].[ExamAssessments]'))
BEGIN
CREATE INDEX [IX_ExamAssessments_ExamBoardId] ON [dbo].[ExamAssessments]([ExamBoardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAssessmentAspects_AspectId_Aspects' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
ALTER TABLE [dbo].[ExamAssessmentAspects]
    ADD CONSTRAINT [FK_ExamAssessmentAspects_AspectId_Aspects] FOREIGN KEY ([AspectId]) REFERENCES [dbo].[Aspects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAssessmentAspects_AspectId' AND object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
CREATE INDEX [IX_ExamAssessmentAspects_AspectId] ON [dbo].[ExamAssessmentAspects]([AspectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAssessmentAspects_AssessmentId_ExamAssessments' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
ALTER TABLE [dbo].[ExamAssessmentAspects]
    ADD CONSTRAINT [FK_ExamAssessmentAspects_AssessmentId_ExamAssessments] FOREIGN KEY ([AssessmentId]) REFERENCES [dbo].[ExamAssessments]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAssessmentAspects_AssessmentId' AND object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
CREATE INDEX [IX_ExamAssessmentAspects_AssessmentId] ON [dbo].[ExamAssessmentAspects]([AssessmentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAssessmentAspects_SeriesId_ExamSeries' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
ALTER TABLE [dbo].[ExamAssessmentAspects]
    ADD CONSTRAINT [FK_ExamAssessmentAspects_SeriesId_ExamSeries] FOREIGN KEY ([SeriesId]) REFERENCES [dbo].[ExamSeries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAssessmentAspects_SeriesId' AND object_id = OBJECT_ID(N'[dbo].[ExamAssessmentAspects]'))
BEGIN
CREATE INDEX [IX_ExamAssessmentAspects_SeriesId] ON [dbo].[ExamAssessmentAspects]([SeriesId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwards_AssessmentId_ExamAssessments' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
ALTER TABLE [dbo].[ExamAwards]
    ADD CONSTRAINT [FK_ExamAwards_AssessmentId_ExamAssessments] FOREIGN KEY ([AssessmentId]) REFERENCES [dbo].[ExamAssessments]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwards_AssessmentId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
CREATE INDEX [IX_ExamAwards_AssessmentId] ON [dbo].[ExamAwards]([AssessmentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwards_QualificationId_ExamQualifications' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
ALTER TABLE [dbo].[ExamAwards]
    ADD CONSTRAINT [FK_ExamAwards_QualificationId_ExamQualifications] FOREIGN KEY ([QualificationId]) REFERENCES [dbo].[ExamQualifications]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwards_QualificationId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
CREATE INDEX [IX_ExamAwards_QualificationId] ON [dbo].[ExamAwards]([QualificationId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwards_CourseId_Courses' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
ALTER TABLE [dbo].[ExamAwards]
    ADD CONSTRAINT [FK_ExamAwards_CourseId_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwards_CourseId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwards]'))
BEGIN
CREATE INDEX [IX_ExamAwards_CourseId] ON [dbo].[ExamAwards]([CourseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwardElements_AwardId_ExamAwards' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwardElements]'))
BEGIN
ALTER TABLE [dbo].[ExamAwardElements]
    ADD CONSTRAINT [FK_ExamAwardElements_AwardId_ExamAwards] FOREIGN KEY ([AwardId]) REFERENCES [dbo].[ExamAwards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwardElements_AwardId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwardElements]'))
BEGIN
CREATE INDEX [IX_ExamAwardElements_AwardId] ON [dbo].[ExamAwardElements]([AwardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwardElements_ElementId_ExamElements' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwardElements]'))
BEGIN
ALTER TABLE [dbo].[ExamAwardElements]
    ADD CONSTRAINT [FK_ExamAwardElements_ElementId_ExamElements] FOREIGN KEY ([ElementId]) REFERENCES [dbo].[ExamElements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwardElements_ElementId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwardElements]'))
BEGIN
CREATE INDEX [IX_ExamAwardElements_ElementId] ON [dbo].[ExamAwardElements]([ElementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwardSeries_AwardId_ExamAwards' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwardSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamAwardSeries]
    ADD CONSTRAINT [FK_ExamAwardSeries_AwardId_ExamAwards] FOREIGN KEY ([AwardId]) REFERENCES [dbo].[ExamAwards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwardSeries_AwardId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwardSeries]'))
BEGIN
CREATE INDEX [IX_ExamAwardSeries_AwardId] ON [dbo].[ExamAwardSeries]([AwardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamAwardSeries_SeriesId_ExamSeries' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamAwardSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamAwardSeries]
    ADD CONSTRAINT [FK_ExamAwardSeries_SeriesId_ExamSeries] FOREIGN KEY ([SeriesId]) REFERENCES [dbo].[ExamSeries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamAwardSeries_SeriesId' AND object_id = OBJECT_ID(N'[dbo].[ExamAwardSeries]'))
BEGIN
CREATE INDEX [IX_ExamAwardSeries_SeriesId] ON [dbo].[ExamAwardSeries]([SeriesId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamBaseComponents_AssessmentModeId_ExamAssessmentModes' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamBaseComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamBaseComponents]
    ADD CONSTRAINT [FK_ExamBaseComponents_AssessmentModeId_ExamAssessmentModes] FOREIGN KEY ([AssessmentModeId]) REFERENCES [dbo].[ExamAssessmentModes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamBaseComponents_AssessmentModeId' AND object_id = OBJECT_ID(N'[dbo].[ExamBaseComponents]'))
BEGIN
CREATE INDEX [IX_ExamBaseComponents_AssessmentModeId] ON [dbo].[ExamBaseComponents]([AssessmentModeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamBaseComponents_ExamAssessmentId_ExamAssessments' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamBaseComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamBaseComponents]
    ADD CONSTRAINT [FK_ExamBaseComponents_ExamAssessmentId_ExamAssessments] FOREIGN KEY ([ExamAssessmentId]) REFERENCES [dbo].[ExamAssessments]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamBaseComponents_ExamAssessmentId' AND object_id = OBJECT_ID(N'[dbo].[ExamBaseComponents]'))
BEGIN
CREATE INDEX [IX_ExamBaseComponents_ExamAssessmentId] ON [dbo].[ExamBaseComponents]([ExamAssessmentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamBaseElements_AssessmentId_ExamAssessments' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
ALTER TABLE [dbo].[ExamBaseElements]
    ADD CONSTRAINT [FK_ExamBaseElements_AssessmentId_ExamAssessments] FOREIGN KEY ([AssessmentId]) REFERENCES [dbo].[ExamAssessments]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamBaseElements_AssessmentId' AND object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
CREATE INDEX [IX_ExamBaseElements_AssessmentId] ON [dbo].[ExamBaseElements]([AssessmentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamBaseElements_QcaCodeId_SubjectCodes' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
ALTER TABLE [dbo].[ExamBaseElements]
    ADD CONSTRAINT [FK_ExamBaseElements_QcaCodeId_SubjectCodes] FOREIGN KEY ([QcaCodeId]) REFERENCES [dbo].[SubjectCodes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamBaseElements_QcaCodeId' AND object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
CREATE INDEX [IX_ExamBaseElements_QcaCodeId] ON [dbo].[ExamBaseElements]([QcaCodeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamBaseElements_LevelId_ExamQualificationLevels' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
ALTER TABLE [dbo].[ExamBaseElements]
    ADD CONSTRAINT [FK_ExamBaseElements_LevelId_ExamQualificationLevels] FOREIGN KEY ([LevelId]) REFERENCES [dbo].[ExamQualificationLevels]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamBaseElements_LevelId' AND object_id = OBJECT_ID(N'[dbo].[ExamBaseElements]'))
BEGIN
CREATE INDEX [IX_ExamBaseElements_LevelId] ON [dbo].[ExamBaseElements]([LevelId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamCandidate_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamCandidate]'))
BEGIN
ALTER TABLE [dbo].[ExamCandidate]
    ADD CONSTRAINT [FK_ExamCandidate_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamCandidate_StudentId' AND object_id = OBJECT_ID(N'[dbo].[ExamCandidate]'))
BEGIN
CREATE INDEX [IX_ExamCandidate_StudentId] ON [dbo].[ExamCandidate]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamCandidateSeries_SeriesId_ExamSeries' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamCandidateSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamCandidateSeries]
    ADD CONSTRAINT [FK_ExamCandidateSeries_SeriesId_ExamSeries] FOREIGN KEY ([SeriesId]) REFERENCES [dbo].[ExamSeries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamCandidateSeries_SeriesId' AND object_id = OBJECT_ID(N'[dbo].[ExamCandidateSeries]'))
BEGIN
CREATE INDEX [IX_ExamCandidateSeries_SeriesId] ON [dbo].[ExamCandidateSeries]([SeriesId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamCandidateSeries_CandidateId_ExamCandidate' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamCandidateSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamCandidateSeries]
    ADD CONSTRAINT [FK_ExamCandidateSeries_CandidateId_ExamCandidate] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[ExamCandidate]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamCandidateSeries_CandidateId' AND object_id = OBJECT_ID(N'[dbo].[ExamCandidateSeries]'))
BEGIN
CREATE INDEX [IX_ExamCandidateSeries_CandidateId] ON [dbo].[ExamCandidateSeries]([CandidateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamCandidateSpecialArrangements_CandidateId_ExamCandidate' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamCandidateSpecialArrangements]'))
BEGIN
ALTER TABLE [dbo].[ExamCandidateSpecialArrangements]
    ADD CONSTRAINT [FK_ExamCandidateSpecialArrangements_CandidateId_ExamCandidate] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[ExamCandidate]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamCandidateSpecialArrangements_CandidateId' AND object_id = OBJECT_ID(N'[dbo].[ExamCandidateSpecialArrangements]'))
BEGIN
CREATE INDEX [IX_ExamCandidateSpecialArrangements_CandidateId] ON [dbo].[ExamCandidateSpecialArrangements]([CandidateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamCandidateSpecialArrangements_SpecialArrangementId_ExamSpecialArrangements' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamCandidateSpecialArrangements]'))
BEGIN
ALTER TABLE [dbo].[ExamCandidateSpecialArrangements]
    ADD CONSTRAINT [FK_ExamCandidateSpecialArrangements_SpecialArrangementId_ExamSpecialArrangements] FOREIGN KEY ([SpecialArrangementId]) REFERENCES [dbo].[ExamSpecialArrangements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamCandidateSpecialArrangements_SpecialArrangementId' AND object_id = OBJECT_ID(N'[dbo].[ExamCandidateSpecialArrangements]'))
BEGIN
CREATE INDEX [IX_ExamCandidateSpecialArrangements_SpecialArrangementId] ON [dbo].[ExamCandidateSpecialArrangements]([SpecialArrangementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamComponents_BaseComponentId_ExamBaseComponents' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamComponents]
    ADD CONSTRAINT [FK_ExamComponents_BaseComponentId_ExamBaseComponents] FOREIGN KEY ([BaseComponentId]) REFERENCES [dbo].[ExamBaseComponents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamComponents_BaseComponentId' AND object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
CREATE INDEX [IX_ExamComponents_BaseComponentId] ON [dbo].[ExamComponents]([BaseComponentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamComponents_ExamSeriesId_ExamSeries' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamComponents]
    ADD CONSTRAINT [FK_ExamComponents_ExamSeriesId_ExamSeries] FOREIGN KEY ([ExamSeriesId]) REFERENCES [dbo].[ExamSeries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamComponents_ExamSeriesId' AND object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
CREATE INDEX [IX_ExamComponents_ExamSeriesId] ON [dbo].[ExamComponents]([ExamSeriesId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamComponents_AssessmentModeId_ExamAssessmentModes' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamComponents]
    ADD CONSTRAINT [FK_ExamComponents_AssessmentModeId_ExamAssessmentModes] FOREIGN KEY ([AssessmentModeId]) REFERENCES [dbo].[ExamAssessmentModes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamComponents_AssessmentModeId' AND object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
CREATE INDEX [IX_ExamComponents_AssessmentModeId] ON [dbo].[ExamComponents]([AssessmentModeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamComponents_ExamDateId_ExamDates' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamComponents]
    ADD CONSTRAINT [FK_ExamComponents_ExamDateId_ExamDates] FOREIGN KEY ([ExamDateId]) REFERENCES [dbo].[ExamDates]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamComponents_ExamDateId' AND object_id = OBJECT_ID(N'[dbo].[ExamComponents]'))
BEGIN
CREATE INDEX [IX_ExamComponents_ExamDateId] ON [dbo].[ExamComponents]([ExamDateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamDates_SessionId_ExamSessions' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamDates]'))
BEGIN
ALTER TABLE [dbo].[ExamDates]
    ADD CONSTRAINT [FK_ExamDates_SessionId_ExamSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[ExamSessions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamDates_SessionId' AND object_id = OBJECT_ID(N'[dbo].[ExamDates]'))
BEGIN
CREATE INDEX [IX_ExamDates_SessionId] ON [dbo].[ExamDates]([SessionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamElements_BaseElementId_ExamBaseElements' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamElements]'))
BEGIN
ALTER TABLE [dbo].[ExamElements]
    ADD CONSTRAINT [FK_ExamElements_BaseElementId_ExamBaseElements] FOREIGN KEY ([BaseElementId]) REFERENCES [dbo].[ExamBaseElements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamElements_BaseElementId' AND object_id = OBJECT_ID(N'[dbo].[ExamElements]'))
BEGIN
CREATE INDEX [IX_ExamElements_BaseElementId] ON [dbo].[ExamElements]([BaseElementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamElements_SeriesId_ExamSeries' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamElements]'))
BEGIN
ALTER TABLE [dbo].[ExamElements]
    ADD CONSTRAINT [FK_ExamElements_SeriesId_ExamSeries] FOREIGN KEY ([SeriesId]) REFERENCES [dbo].[ExamSeries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamElements_SeriesId' AND object_id = OBJECT_ID(N'[dbo].[ExamElements]'))
BEGIN
CREATE INDEX [IX_ExamElements_SeriesId] ON [dbo].[ExamElements]([SeriesId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamElementComponents_ElementId_ExamElements' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamElementComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamElementComponents]
    ADD CONSTRAINT [FK_ExamElementComponents_ElementId_ExamElements] FOREIGN KEY ([ElementId]) REFERENCES [dbo].[ExamElements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamElementComponents_ElementId' AND object_id = OBJECT_ID(N'[dbo].[ExamElementComponents]'))
BEGIN
CREATE INDEX [IX_ExamElementComponents_ElementId] ON [dbo].[ExamElementComponents]([ElementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamElementComponents_ComponentId_ExamComponents' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamElementComponents]'))
BEGIN
ALTER TABLE [dbo].[ExamElementComponents]
    ADD CONSTRAINT [FK_ExamElementComponents_ComponentId_ExamComponents] FOREIGN KEY ([ComponentId]) REFERENCES [dbo].[ExamComponents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamElementComponents_ComponentId' AND object_id = OBJECT_ID(N'[dbo].[ExamElementComponents]'))
BEGIN
CREATE INDEX [IX_ExamElementComponents_ComponentId] ON [dbo].[ExamElementComponents]([ComponentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamEnrolments_AwardId_ExamAwards' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamEnrolments]'))
BEGIN
ALTER TABLE [dbo].[ExamEnrolments]
    ADD CONSTRAINT [FK_ExamEnrolments_AwardId_ExamAwards] FOREIGN KEY ([AwardId]) REFERENCES [dbo].[ExamAwards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamEnrolments_AwardId' AND object_id = OBJECT_ID(N'[dbo].[ExamEnrolments]'))
BEGIN
CREATE INDEX [IX_ExamEnrolments_AwardId] ON [dbo].[ExamEnrolments]([AwardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamEnrolments_CandidateId_ExamCandidate' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamEnrolments]'))
BEGIN
ALTER TABLE [dbo].[ExamEnrolments]
    ADD CONSTRAINT [FK_ExamEnrolments_CandidateId_ExamCandidate] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[ExamCandidate]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamEnrolments_CandidateId' AND object_id = OBJECT_ID(N'[dbo].[ExamEnrolments]'))
BEGIN
CREATE INDEX [IX_ExamEnrolments_CandidateId] ON [dbo].[ExamEnrolments]([CandidateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamQualificationLevels_DefaultGradeSetId_GradeSets' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamQualificationLevels]'))
BEGIN
ALTER TABLE [dbo].[ExamQualificationLevels]
    ADD CONSTRAINT [FK_ExamQualificationLevels_DefaultGradeSetId_GradeSets] FOREIGN KEY ([DefaultGradeSetId]) REFERENCES [dbo].[GradeSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamQualificationLevels_DefaultGradeSetId' AND object_id = OBJECT_ID(N'[dbo].[ExamQualificationLevels]'))
BEGIN
CREATE INDEX [IX_ExamQualificationLevels_DefaultGradeSetId] ON [dbo].[ExamQualificationLevels]([DefaultGradeSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamQualificationLevels_QualificationId_ExamQualifications' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamQualificationLevels]'))
BEGIN
ALTER TABLE [dbo].[ExamQualificationLevels]
    ADD CONSTRAINT [FK_ExamQualificationLevels_QualificationId_ExamQualifications] FOREIGN KEY ([QualificationId]) REFERENCES [dbo].[ExamQualifications]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamQualificationLevels_QualificationId' AND object_id = OBJECT_ID(N'[dbo].[ExamQualificationLevels]'))
BEGIN
CREATE INDEX [IX_ExamQualificationLevels_QualificationId] ON [dbo].[ExamQualificationLevels]([QualificationId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamResultEmbares_ResultSetId_ResultSets' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamResultEmbares]'))
BEGIN
ALTER TABLE [dbo].[ExamResultEmbares]
    ADD CONSTRAINT [FK_ExamResultEmbares_ResultSetId_ResultSets] FOREIGN KEY ([ResultSetId]) REFERENCES [dbo].[ResultSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamResultEmbares_ResultSetId' AND object_id = OBJECT_ID(N'[dbo].[ExamResultEmbares]'))
BEGIN
CREATE INDEX [IX_ExamResultEmbares_ResultSetId] ON [dbo].[ExamResultEmbares]([ResultSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamRooms_RoomId_Rooms' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamRooms]'))
BEGIN
ALTER TABLE [dbo].[ExamRooms]
    ADD CONSTRAINT [FK_ExamRooms_RoomId_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamRooms_RoomId' AND object_id = OBJECT_ID(N'[dbo].[ExamRooms]'))
BEGIN
CREATE INDEX [IX_ExamRooms_RoomId] ON [dbo].[ExamRooms]([RoomId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamRoomSeatBlocks_ExamRoomId_ExamRooms' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamRoomSeatBlocks]'))
BEGIN
ALTER TABLE [dbo].[ExamRoomSeatBlocks]
    ADD CONSTRAINT [FK_ExamRoomSeatBlocks_ExamRoomId_ExamRooms] FOREIGN KEY ([ExamRoomId]) REFERENCES [dbo].[ExamRooms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamRoomSeatBlocks_ExamRoomId' AND object_id = OBJECT_ID(N'[dbo].[ExamRoomSeatBlocks]'))
BEGIN
CREATE INDEX [IX_ExamRoomSeatBlocks_ExamRoomId] ON [dbo].[ExamRoomSeatBlocks]([ExamRoomId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamSeasons_ResultSetId_ResultSets' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamSeasons]'))
BEGIN
ALTER TABLE [dbo].[ExamSeasons]
    ADD CONSTRAINT [FK_ExamSeasons_ResultSetId_ResultSets] FOREIGN KEY ([ResultSetId]) REFERENCES [dbo].[ResultSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSeasons_ResultSetId' AND object_id = OBJECT_ID(N'[dbo].[ExamSeasons]'))
BEGIN
CREATE INDEX [IX_ExamSeasons_ResultSetId] ON [dbo].[ExamSeasons]([ResultSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamSeatAllocations_SittingId_ExamComponentSittings' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamSeatAllocations]'))
BEGIN
ALTER TABLE [dbo].[ExamSeatAllocations]
    ADD CONSTRAINT [FK_ExamSeatAllocations_SittingId_ExamComponentSittings] FOREIGN KEY ([SittingId]) REFERENCES [dbo].[ExamComponentSittings]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSeatAllocations_SittingId' AND object_id = OBJECT_ID(N'[dbo].[ExamSeatAllocations]'))
BEGIN
CREATE INDEX [IX_ExamSeatAllocations_SittingId] ON [dbo].[ExamSeatAllocations]([SittingId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamSeatAllocations_CandidateId_ExamCandidate' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamSeatAllocations]'))
BEGIN
ALTER TABLE [dbo].[ExamSeatAllocations]
    ADD CONSTRAINT [FK_ExamSeatAllocations_CandidateId_ExamCandidate] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[ExamCandidate]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSeatAllocations_CandidateId' AND object_id = OBJECT_ID(N'[dbo].[ExamSeatAllocations]'))
BEGIN
CREATE INDEX [IX_ExamSeatAllocations_CandidateId] ON [dbo].[ExamSeatAllocations]([CandidateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamSeries_ExamSeasonId_ExamSeasons' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamSeries]
    ADD CONSTRAINT [FK_ExamSeries_ExamSeasonId_ExamSeasons] FOREIGN KEY ([ExamSeasonId]) REFERENCES [dbo].[ExamSeasons]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSeries_ExamSeasonId' AND object_id = OBJECT_ID(N'[dbo].[ExamSeries]'))
BEGIN
CREATE INDEX [IX_ExamSeries_ExamSeasonId] ON [dbo].[ExamSeries]([ExamSeasonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExamSeries_ExamBoardId_ExamBoards' AND parent_object_id = OBJECT_ID(N'[dbo].[ExamSeries]'))
BEGIN
ALTER TABLE [dbo].[ExamSeries]
    ADD CONSTRAINT [FK_ExamSeries_ExamBoardId_ExamBoards] FOREIGN KEY ([ExamBoardId]) REFERENCES [dbo].[ExamBoards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSeries_ExamBoardId' AND object_id = OBJECT_ID(N'[dbo].[ExamSeries]'))
BEGIN
CREATE INDEX [IX_ExamSeries_ExamBoardId] ON [dbo].[ExamSeries]([ExamBoardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Exclusions_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
ALTER TABLE [dbo].[Exclusions]
    ADD CONSTRAINT [FK_Exclusions_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Exclusions_StudentId' AND object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
CREATE INDEX [IX_Exclusions_StudentId] ON [dbo].[Exclusions]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Exclusions_ExclusionTypeId_ExclusionTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
ALTER TABLE [dbo].[Exclusions]
    ADD CONSTRAINT [FK_Exclusions_ExclusionTypeId_ExclusionTypes] FOREIGN KEY ([ExclusionTypeId]) REFERENCES [dbo].[ExclusionTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Exclusions_ExclusionTypeId' AND object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
CREATE INDEX [IX_Exclusions_ExclusionTypeId] ON [dbo].[Exclusions]([ExclusionTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Exclusions_ExclusionReasonId_ExclusionReasons' AND parent_object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
ALTER TABLE [dbo].[Exclusions]
    ADD CONSTRAINT [FK_Exclusions_ExclusionReasonId_ExclusionReasons] FOREIGN KEY ([ExclusionReasonId]) REFERENCES [dbo].[ExclusionReasons]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Exclusions_ExclusionReasonId' AND object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
CREATE INDEX [IX_Exclusions_ExclusionReasonId] ON [dbo].[Exclusions]([ExclusionReasonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Exclusions_AppealResultId_ExclusionAppealResults' AND parent_object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
ALTER TABLE [dbo].[Exclusions]
    ADD CONSTRAINT [FK_Exclusions_AppealResultId_ExclusionAppealResults] FOREIGN KEY ([AppealResultId]) REFERENCES [dbo].[ExclusionAppealResults]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Exclusions_AppealResultId' AND object_id = OBJECT_ID(N'[dbo].[Exclusions]'))
BEGIN
CREATE INDEX [IX_Exclusions_AppealResultId] ON [dbo].[Exclusions]([AppealResultId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_GiftedTalentedStudents_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[GiftedTalentedStudents]'))
BEGIN
ALTER TABLE [dbo].[GiftedTalentedStudents]
    ADD CONSTRAINT [FK_GiftedTalentedStudents_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GiftedTalentedStudents_StudentId' AND object_id = OBJECT_ID(N'[dbo].[GiftedTalentedStudents]'))
BEGIN
CREATE INDEX [IX_GiftedTalentedStudents_StudentId] ON [dbo].[GiftedTalentedStudents]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_GiftedTalentedStudents_SubjectId_Subjects' AND parent_object_id = OBJECT_ID(N'[dbo].[GiftedTalentedStudents]'))
BEGIN
ALTER TABLE [dbo].[GiftedTalentedStudents]
    ADD CONSTRAINT [FK_GiftedTalentedStudents_SubjectId_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GiftedTalentedStudents_SubjectId' AND object_id = OBJECT_ID(N'[dbo].[GiftedTalentedStudents]'))
BEGIN
CREATE INDEX [IX_GiftedTalentedStudents_SubjectId] ON [dbo].[GiftedTalentedStudents]([SubjectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Grades_GradeSetId_GradeSets' AND parent_object_id = OBJECT_ID(N'[dbo].[Grades]'))
BEGIN
ALTER TABLE [dbo].[Grades]
    ADD CONSTRAINT [FK_Grades_GradeSetId_GradeSets] FOREIGN KEY ([GradeSetId]) REFERENCES [dbo].[GradeSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Grades_GradeSetId' AND object_id = OBJECT_ID(N'[dbo].[Grades]'))
BEGIN
CREATE INDEX [IX_Grades_GradeSetId] ON [dbo].[Grades]([GradeSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HomeworkItems_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[HomeworkItems]'))
BEGIN
ALTER TABLE [dbo].[HomeworkItems]
    ADD CONSTRAINT [FK_HomeworkItems_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomeworkItems_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[HomeworkItems]'))
BEGIN
CREATE INDEX [IX_HomeworkItems_DirectoryId] ON [dbo].[HomeworkItems]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HomeworkSubmissions_HomeworkItemId_HomeworkItems' AND parent_object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
ALTER TABLE [dbo].[HomeworkSubmissions]
    ADD CONSTRAINT [FK_HomeworkSubmissions_HomeworkItemId_HomeworkItems] FOREIGN KEY ([HomeworkItemId]) REFERENCES [dbo].[HomeworkItems]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomeworkSubmissions_HomeworkItemId' AND object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
CREATE INDEX [IX_HomeworkSubmissions_HomeworkItemId] ON [dbo].[HomeworkSubmissions]([HomeworkItemId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HomeworkSubmissions_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
ALTER TABLE [dbo].[HomeworkSubmissions]
    ADD CONSTRAINT [FK_HomeworkSubmissions_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomeworkSubmissions_StudentId' AND object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
CREATE INDEX [IX_HomeworkSubmissions_StudentId] ON [dbo].[HomeworkSubmissions]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HomeworkSubmissions_TaskId_Tasks' AND parent_object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
ALTER TABLE [dbo].[HomeworkSubmissions]
    ADD CONSTRAINT [FK_HomeworkSubmissions_TaskId_Tasks] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Tasks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomeworkSubmissions_TaskId' AND object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
CREATE INDEX [IX_HomeworkSubmissions_TaskId] ON [dbo].[HomeworkSubmissions]([TaskId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HomeworkSubmissions_DocumentId_Documents' AND parent_object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
ALTER TABLE [dbo].[HomeworkSubmissions]
    ADD CONSTRAINT [FK_HomeworkSubmissions_DocumentId_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomeworkSubmissions_DocumentId' AND object_id = OBJECT_ID(N'[dbo].[HomeworkSubmissions]'))
BEGIN
CREATE INDEX [IX_HomeworkSubmissions_DocumentId] ON [dbo].[HomeworkSubmissions]([DocumentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Houses_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[Houses]'))
BEGIN
ALTER TABLE [dbo].[Houses]
    ADD CONSTRAINT [FK_Houses_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Houses_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[Houses]'))
BEGIN
CREATE INDEX [IX_Houses_StudentGroupId] ON [dbo].[Houses]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Incidents_IncidentTypeId_IncidentTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
ALTER TABLE [dbo].[Incidents]
    ADD CONSTRAINT [FK_Incidents_IncidentTypeId_IncidentTypes] FOREIGN KEY ([IncidentTypeId]) REFERENCES [dbo].[IncidentTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Incidents_IncidentTypeId' AND object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
CREATE INDEX [IX_Incidents_IncidentTypeId] ON [dbo].[Incidents]([IncidentTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Incidents_LocationId_Locations' AND parent_object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
ALTER TABLE [dbo].[Incidents]
    ADD CONSTRAINT [FK_Incidents_LocationId_Locations] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Incidents_LocationId' AND object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
CREATE INDEX [IX_Incidents_LocationId] ON [dbo].[Incidents]([LocationId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Incidents_AcademicYearId_AcademicYears' AND parent_object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
ALTER TABLE [dbo].[Incidents]
    ADD CONSTRAINT [FK_Incidents_AcademicYearId_AcademicYears] FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Incidents_AcademicYearId' AND object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
CREATE INDEX [IX_Incidents_AcademicYearId] ON [dbo].[Incidents]([AcademicYearId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Incidents_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
ALTER TABLE [dbo].[Incidents]
    ADD CONSTRAINT [FK_Incidents_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Incidents_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
CREATE INDEX [IX_Incidents_CreatedById] ON [dbo].[Incidents]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Incidents_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
ALTER TABLE [dbo].[Incidents]
    ADD CONSTRAINT [FK_Incidents_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Incidents_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Incidents]'))
BEGIN
CREATE INDEX [IX_Incidents_LastModifiedById] ON [dbo].[Incidents]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlans_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
ALTER TABLE [dbo].[LessonPlans]
    ADD CONSTRAINT [FK_LessonPlans_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlans_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
CREATE INDEX [IX_LessonPlans_DirectoryId] ON [dbo].[LessonPlans]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlans_StudyTopicId_StudyTopics' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
ALTER TABLE [dbo].[LessonPlans]
    ADD CONSTRAINT [FK_LessonPlans_StudyTopicId_StudyTopics] FOREIGN KEY ([StudyTopicId]) REFERENCES [dbo].[StudyTopics]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlans_StudyTopicId' AND object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
CREATE INDEX [IX_LessonPlans_StudyTopicId] ON [dbo].[LessonPlans]([StudyTopicId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlans_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
ALTER TABLE [dbo].[LessonPlans]
    ADD CONSTRAINT [FK_LessonPlans_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlans_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
CREATE INDEX [IX_LessonPlans_CreatedById] ON [dbo].[LessonPlans]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlans_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
ALTER TABLE [dbo].[LessonPlans]
    ADD CONSTRAINT [FK_LessonPlans_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlans_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[LessonPlans]'))
BEGIN
CREATE INDEX [IX_LessonPlans_LastModifiedById] ON [dbo].[LessonPlans]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlanHomeworkItems_LessonPlanId_LessonPlans' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlanHomeworkItems]'))
BEGIN
ALTER TABLE [dbo].[LessonPlanHomeworkItems]
    ADD CONSTRAINT [FK_LessonPlanHomeworkItems_LessonPlanId_LessonPlans] FOREIGN KEY ([LessonPlanId]) REFERENCES [dbo].[LessonPlans]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlanHomeworkItems_LessonPlanId' AND object_id = OBJECT_ID(N'[dbo].[LessonPlanHomeworkItems]'))
BEGIN
CREATE INDEX [IX_LessonPlanHomeworkItems_LessonPlanId] ON [dbo].[LessonPlanHomeworkItems]([LessonPlanId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LessonPlanHomeworkItems_HomeworkItemId_HomeworkItems' AND parent_object_id = OBJECT_ID(N'[dbo].[LessonPlanHomeworkItems]'))
BEGIN
ALTER TABLE [dbo].[LessonPlanHomeworkItems]
    ADD CONSTRAINT [FK_LessonPlanHomeworkItems_HomeworkItemId_HomeworkItems] FOREIGN KEY ([HomeworkItemId]) REFERENCES [dbo].[HomeworkItems]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LessonPlanHomeworkItems_HomeworkItemId' AND object_id = OBJECT_ID(N'[dbo].[LessonPlanHomeworkItems]'))
BEGIN
CREATE INDEX [IX_LessonPlanHomeworkItems_HomeworkItemId] ON [dbo].[LessonPlanHomeworkItems]([HomeworkItemId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LogNotes_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
ALTER TABLE [dbo].[LogNotes]
    ADD CONSTRAINT [FK_LogNotes_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LogNotes_StudentId' AND object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
CREATE INDEX [IX_LogNotes_StudentId] ON [dbo].[LogNotes]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LogNotes_AcademicYearId_AcademicYears' AND parent_object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
ALTER TABLE [dbo].[LogNotes]
    ADD CONSTRAINT [FK_LogNotes_AcademicYearId_AcademicYears] FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LogNotes_AcademicYearId' AND object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
CREATE INDEX [IX_LogNotes_AcademicYearId] ON [dbo].[LogNotes]([AcademicYearId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LogNotes_LogNoteTypeId_LogNoteTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
ALTER TABLE [dbo].[LogNotes]
    ADD CONSTRAINT [FK_LogNotes_LogNoteTypeId_LogNoteTypes] FOREIGN KEY ([LogNoteTypeId]) REFERENCES [dbo].[LogNoteTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LogNotes_LogNoteTypeId' AND object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
CREATE INDEX [IX_LogNotes_LogNoteTypeId] ON [dbo].[LogNotes]([LogNoteTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LogNotes_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
ALTER TABLE [dbo].[LogNotes]
    ADD CONSTRAINT [FK_LogNotes_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LogNotes_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
CREATE INDEX [IX_LogNotes_CreatedById] ON [dbo].[LogNotes]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LogNotes_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
ALTER TABLE [dbo].[LogNotes]
    ADD CONSTRAINT [FK_LogNotes_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LogNotes_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[LogNotes]'))
BEGIN
CREATE INDEX [IX_LogNotes_LastModifiedById] ON [dbo].[LogNotes]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Marksheets_MarksheetTemplateId_MarksheetTemplates' AND parent_object_id = OBJECT_ID(N'[dbo].[Marksheets]'))
BEGIN
ALTER TABLE [dbo].[Marksheets]
    ADD CONSTRAINT [FK_Marksheets_MarksheetTemplateId_MarksheetTemplates] FOREIGN KEY ([MarksheetTemplateId]) REFERENCES [dbo].[MarksheetTemplates]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Marksheets_MarksheetTemplateId' AND object_id = OBJECT_ID(N'[dbo].[Marksheets]'))
BEGIN
CREATE INDEX [IX_Marksheets_MarksheetTemplateId] ON [dbo].[Marksheets]([MarksheetTemplateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Marksheets_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[Marksheets]'))
BEGIN
ALTER TABLE [dbo].[Marksheets]
    ADD CONSTRAINT [FK_Marksheets_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Marksheets_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[Marksheets]'))
BEGIN
CREATE INDEX [IX_Marksheets_StudentGroupId] ON [dbo].[Marksheets]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MarksheetColumns_TemplateId_MarksheetTemplates' AND parent_object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
ALTER TABLE [dbo].[MarksheetColumns]
    ADD CONSTRAINT [FK_MarksheetColumns_TemplateId_MarksheetTemplates] FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[MarksheetTemplates]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MarksheetColumns_TemplateId' AND object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
CREATE INDEX [IX_MarksheetColumns_TemplateId] ON [dbo].[MarksheetColumns]([TemplateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MarksheetColumns_AspectId_Aspects' AND parent_object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
ALTER TABLE [dbo].[MarksheetColumns]
    ADD CONSTRAINT [FK_MarksheetColumns_AspectId_Aspects] FOREIGN KEY ([AspectId]) REFERENCES [dbo].[Aspects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MarksheetColumns_AspectId' AND object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
CREATE INDEX [IX_MarksheetColumns_AspectId] ON [dbo].[MarksheetColumns]([AspectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MarksheetColumns_ResultSetId_ResultSets' AND parent_object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
ALTER TABLE [dbo].[MarksheetColumns]
    ADD CONSTRAINT [FK_MarksheetColumns_ResultSetId_ResultSets] FOREIGN KEY ([ResultSetId]) REFERENCES [dbo].[ResultSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MarksheetColumns_ResultSetId' AND object_id = OBJECT_ID(N'[dbo].[MarksheetColumns]'))
BEGIN
CREATE INDEX [IX_MarksheetColumns_ResultSetId] ON [dbo].[MarksheetColumns]([ResultSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MedicalEvents_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
ALTER TABLE [dbo].[MedicalEvents]
    ADD CONSTRAINT [FK_MedicalEvents_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MedicalEvents_PersonId' AND object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
CREATE INDEX [IX_MedicalEvents_PersonId] ON [dbo].[MedicalEvents]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MedicalEvents_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
ALTER TABLE [dbo].[MedicalEvents]
    ADD CONSTRAINT [FK_MedicalEvents_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MedicalEvents_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
CREATE INDEX [IX_MedicalEvents_CreatedById] ON [dbo].[MedicalEvents]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MedicalEvents_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
ALTER TABLE [dbo].[MedicalEvents]
    ADD CONSTRAINT [FK_MedicalEvents_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MedicalEvents_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[MedicalEvents]'))
BEGIN
CREATE INDEX [IX_MedicalEvents_LastModifiedById] ON [dbo].[MedicalEvents]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_StaffMemberId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_StaffMemberId_StaffMembers] FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_StaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_StaffMemberId] ON [dbo].[NextOfKin]([StaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_NextOfKinPersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_NextOfKinPersonId_People] FOREIGN KEY ([NextOfKinPersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_NextOfKinPersonId' AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_NextOfKinPersonId] ON [dbo].[NextOfKin]([NextOfKinPersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_RelationshipTypeId_NextOfKinRelationshipTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_RelationshipTypeId_NextOfKinRelationshipTypes] FOREIGN KEY ([RelationshipTypeId]) REFERENCES [dbo].[NextOfKinRelationshipTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_RelationshipTypeId' AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_RelationshipTypeId] ON [dbo].[NextOfKin]([RelationshipTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Observations_ObserveeId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
ALTER TABLE [dbo].[Observations]
    ADD CONSTRAINT [FK_Observations_ObserveeId_StaffMembers] FOREIGN KEY ([ObserveeId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Observations_ObserveeId' AND object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
CREATE INDEX [IX_Observations_ObserveeId] ON [dbo].[Observations]([ObserveeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Observations_ObserverId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
ALTER TABLE [dbo].[Observations]
    ADD CONSTRAINT [FK_Observations_ObserverId_StaffMembers] FOREIGN KEY ([ObserverId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Observations_ObserverId' AND object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
CREATE INDEX [IX_Observations_ObserverId] ON [dbo].[Observations]([ObserverId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Observations_OutcomeId_ObservationOutcomes' AND parent_object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
ALTER TABLE [dbo].[Observations]
    ADD CONSTRAINT [FK_Observations_OutcomeId_ObservationOutcomes] FOREIGN KEY ([OutcomeId]) REFERENCES [dbo].[ObservationOutcomes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Observations_OutcomeId' AND object_id = OBJECT_ID(N'[dbo].[Observations]'))
BEGIN
CREATE INDEX [IX_Observations_OutcomeId] ON [dbo].[Observations]([OutcomeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEvenings_EventId_DiaryEvents' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEvenings]'))
BEGIN
ALTER TABLE [dbo].[ParentEvenings]
    ADD CONSTRAINT [FK_ParentEvenings_EventId_DiaryEvents] FOREIGN KEY ([EventId]) REFERENCES [dbo].[DiaryEvents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEvenings_EventId' AND object_id = OBJECT_ID(N'[dbo].[ParentEvenings]'))
BEGIN
CREATE INDEX [IX_ParentEvenings_EventId] ON [dbo].[ParentEvenings]([EventId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningAppointments_ParentEveningStaffMemberId_ParentEveningStaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningAppointments]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningAppointments]
    ADD CONSTRAINT [FK_ParentEveningAppointments_ParentEveningStaffMemberId_ParentEveningStaffMembers] FOREIGN KEY ([ParentEveningStaffMemberId]) REFERENCES [dbo].[ParentEveningStaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningAppointments_ParentEveningStaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningAppointments]'))
BEGIN
CREATE INDEX [IX_ParentEveningAppointments_ParentEveningStaffMemberId] ON [dbo].[ParentEveningAppointments]([ParentEveningStaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningAppointments_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningAppointments]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningAppointments]
    ADD CONSTRAINT [FK_ParentEveningAppointments_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningAppointments_StudentId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningAppointments]'))
BEGIN
CREATE INDEX [IX_ParentEveningAppointments_StudentId] ON [dbo].[ParentEveningAppointments]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningGroups_ParentEveningId_ParentEvenings' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningGroups]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningGroups]
    ADD CONSTRAINT [FK_ParentEveningGroups_ParentEveningId_ParentEvenings] FOREIGN KEY ([ParentEveningId]) REFERENCES [dbo].[ParentEvenings]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningGroups_ParentEveningId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningGroups]'))
BEGIN
CREATE INDEX [IX_ParentEveningGroups_ParentEveningId] ON [dbo].[ParentEveningGroups]([ParentEveningId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningGroups_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningGroups]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningGroups]
    ADD CONSTRAINT [FK_ParentEveningGroups_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningGroups_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningGroups]'))
BEGIN
CREATE INDEX [IX_ParentEveningGroups_StudentGroupId] ON [dbo].[ParentEveningGroups]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningStaffMembers_ParentEveningId_ParentEvenings' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningStaffMembers]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningStaffMembers]
    ADD CONSTRAINT [FK_ParentEveningStaffMembers_ParentEveningId_ParentEvenings] FOREIGN KEY ([ParentEveningId]) REFERENCES [dbo].[ParentEvenings]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningStaffMembers_ParentEveningId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningStaffMembers]'))
BEGIN
CREATE INDEX [IX_ParentEveningStaffMembers_ParentEveningId] ON [dbo].[ParentEveningStaffMembers]([ParentEveningId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ParentEveningStaffMembers_StaffMemberId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[ParentEveningStaffMembers]'))
BEGIN
ALTER TABLE [dbo].[ParentEveningStaffMembers]
    ADD CONSTRAINT [FK_ParentEveningStaffMembers_StaffMemberId_StaffMembers] FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ParentEveningStaffMembers_StaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[ParentEveningStaffMembers]'))
BEGIN
CREATE INDEX [IX_ParentEveningStaffMembers_StaffMemberId] ON [dbo].[ParentEveningStaffMembers]([StaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_PhotoId_Photos' AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_PhotoId_Photos] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[Photos]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_PhotoId' AND object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
CREATE INDEX [IX_People_PhotoId] ON [dbo].[People]([PhotoId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_EthnicityId_Ethnicities' AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_EthnicityId_Ethnicities] FOREIGN KEY ([EthnicityId]) REFERENCES [dbo].[Ethnicities]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_EthnicityId' AND object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
CREATE INDEX [IX_People_EthnicityId] ON [dbo].[People]([EthnicityId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_DirectoryId_Directories' AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_DirectoryId_Directories] FOREIGN KEY ([DirectoryId]) REFERENCES [dbo].[Directories]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_DirectoryId' AND object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
CREATE INDEX [IX_People_DirectoryId] ON [dbo].[People]([DirectoryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
CREATE INDEX [IX_People_CreatedById] ON [dbo].[People]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
CREATE INDEX [IX_People_LastModifiedById] ON [dbo].[People]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PersonConditions_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[PersonConditions]'))
BEGIN
ALTER TABLE [dbo].[PersonConditions]
    ADD CONSTRAINT [FK_PersonConditions_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonConditions_PersonId' AND object_id = OBJECT_ID(N'[dbo].[PersonConditions]'))
BEGIN
CREATE INDEX [IX_PersonConditions_PersonId] ON [dbo].[PersonConditions]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PersonConditions_MedicalConditionId_MedicalConditions' AND parent_object_id = OBJECT_ID(N'[dbo].[PersonConditions]'))
BEGIN
ALTER TABLE [dbo].[PersonConditions]
    ADD CONSTRAINT [FK_PersonConditions_MedicalConditionId_MedicalConditions] FOREIGN KEY ([MedicalConditionId]) REFERENCES [dbo].[MedicalConditions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonConditions_MedicalConditionId' AND object_id = OBJECT_ID(N'[dbo].[PersonConditions]'))
BEGIN
CREATE INDEX [IX_PersonConditions_MedicalConditionId] ON [dbo].[PersonConditions]([MedicalConditionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PersonDietaryRequirements_DietaryRequirementId_DietaryRequirements' AND parent_object_id = OBJECT_ID(N'[dbo].[PersonDietaryRequirements]'))
BEGIN
ALTER TABLE [dbo].[PersonDietaryRequirements]
    ADD CONSTRAINT [FK_PersonDietaryRequirements_DietaryRequirementId_DietaryRequirements] FOREIGN KEY ([DietaryRequirementId]) REFERENCES [dbo].[DietaryRequirements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonDietaryRequirements_DietaryRequirementId' AND object_id = OBJECT_ID(N'[dbo].[PersonDietaryRequirements]'))
BEGIN
CREATE INDEX [IX_PersonDietaryRequirements_DietaryRequirementId] ON [dbo].[PersonDietaryRequirements]([DietaryRequirementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PersonDietaryRequirements_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[PersonDietaryRequirements]'))
BEGIN
ALTER TABLE [dbo].[PersonDietaryRequirements]
    ADD CONSTRAINT [FK_PersonDietaryRequirements_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonDietaryRequirements_PersonId' AND object_id = OBJECT_ID(N'[dbo].[PersonDietaryRequirements]'))
BEGIN
CREATE INDEX [IX_PersonDietaryRequirements_PersonId] ON [dbo].[PersonDietaryRequirements]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_TypeId_PhoneNumberTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_TypeId_PhoneNumberTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[PhoneNumberTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhoneNumbers_TypeId' AND object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
CREATE INDEX [IX_PhoneNumbers_TypeId] ON [dbo].[PhoneNumbers]([TypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhoneNumbers_PersonId' AND object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
CREATE INDEX [IX_PhoneNumbers_PersonId] ON [dbo].[PhoneNumbers]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_AgencyId_Agencies' AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_AgencyId_Agencies] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhoneNumbers_AgencyId' AND object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
CREATE INDEX [IX_PhoneNumbers_AgencyId] ON [dbo].[PhoneNumbers]([AgencyId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_ProductTypeId_ProductTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Products]'))
BEGIN
ALTER TABLE [dbo].[Products]
    ADD CONSTRAINT [FK_Products_ProductTypeId_ProductTypes] FOREIGN KEY ([ProductTypeId]) REFERENCES [dbo].[ProductTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_ProductTypeId' AND object_id = OBJECT_ID(N'[dbo].[Products]'))
BEGIN
CREATE INDEX [IX_Products_ProductTypeId] ON [dbo].[Products]([ProductTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_VatRateId_VatRates' AND parent_object_id = OBJECT_ID(N'[dbo].[Products]'))
BEGIN
ALTER TABLE [dbo].[Products]
    ADD CONSTRAINT [FK_Products_VatRateId_VatRates] FOREIGN KEY ([VatRateId]) REFERENCES [dbo].[VatRates]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_VatRateId' AND object_id = OBJECT_ID(N'[dbo].[Products]'))
BEGIN
CREATE INDEX [IX_Products_VatRateId] ON [dbo].[Products]([VatRateId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RegGroups_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[RegGroups]'))
BEGIN
ALTER TABLE [dbo].[RegGroups]
    ADD CONSTRAINT [FK_RegGroups_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RegGroups_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[RegGroups]'))
BEGIN
CREATE INDEX [IX_RegGroups_StudentGroupId] ON [dbo].[RegGroups]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RegGroups_YearGroupId_YearGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[RegGroups]'))
BEGIN
ALTER TABLE [dbo].[RegGroups]
    ADD CONSTRAINT [FK_RegGroups_YearGroupId_YearGroups] FOREIGN KEY ([YearGroupId]) REFERENCES [dbo].[YearGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RegGroups_YearGroupId' AND object_id = OBJECT_ID(N'[dbo].[RegGroups]'))
BEGIN
CREATE INDEX [IX_RegGroups_YearGroupId] ON [dbo].[RegGroups]([YearGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCards_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCards]'))
BEGIN
ALTER TABLE [dbo].[ReportCards]
    ADD CONSTRAINT [FK_ReportCards_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCards_StudentId' AND object_id = OBJECT_ID(N'[dbo].[ReportCards]'))
BEGIN
CREATE INDEX [IX_ReportCards_StudentId] ON [dbo].[ReportCards]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCards_BehaviourTypeId_IncidentTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCards]'))
BEGIN
ALTER TABLE [dbo].[ReportCards]
    ADD CONSTRAINT [FK_ReportCards_BehaviourTypeId_IncidentTypes] FOREIGN KEY ([BehaviourTypeId]) REFERENCES [dbo].[IncidentTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCards_BehaviourTypeId' AND object_id = OBJECT_ID(N'[dbo].[ReportCards]'))
BEGIN
CREATE INDEX [IX_ReportCards_BehaviourTypeId] ON [dbo].[ReportCards]([BehaviourTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardEntries_ReportCardId_ReportCards' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardEntries]
    ADD CONSTRAINT [FK_ReportCardEntries_ReportCardId_ReportCards] FOREIGN KEY ([ReportCardId]) REFERENCES [dbo].[ReportCards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardEntries_ReportCardId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardEntries_ReportCardId] ON [dbo].[ReportCardEntries]([ReportCardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardEntries_AttendanceWeekId_AttendanceWeeks' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardEntries]
    ADD CONSTRAINT [FK_ReportCardEntries_AttendanceWeekId_AttendanceWeeks] FOREIGN KEY ([AttendanceWeekId]) REFERENCES [dbo].[AttendanceWeeks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardEntries_AttendanceWeekId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardEntries_AttendanceWeekId] ON [dbo].[ReportCardEntries]([AttendanceWeekId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardEntries_AttendancePeriodId_AttendancePeriods' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardEntries]
    ADD CONSTRAINT [FK_ReportCardEntries_AttendancePeriodId_AttendancePeriods] FOREIGN KEY ([AttendancePeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardEntries_AttendancePeriodId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardEntries_AttendancePeriodId] ON [dbo].[ReportCardEntries]([AttendancePeriodId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardEntries_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardEntries]
    ADD CONSTRAINT [FK_ReportCardEntries_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardEntries_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardEntries_CreatedById] ON [dbo].[ReportCardEntries]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardEntries_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardEntries]
    ADD CONSTRAINT [FK_ReportCardEntries_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardEntries_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[ReportCardEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardEntries_LastModifiedById] ON [dbo].[ReportCardEntries]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardTargets_ReportCardId_ReportCards' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardTargets]'))
BEGIN
ALTER TABLE [dbo].[ReportCardTargets]
    ADD CONSTRAINT [FK_ReportCardTargets_ReportCardId_ReportCards] FOREIGN KEY ([ReportCardId]) REFERENCES [dbo].[ReportCards]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardTargets_ReportCardId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardTargets]'))
BEGIN
CREATE INDEX [IX_ReportCardTargets_ReportCardId] ON [dbo].[ReportCardTargets]([ReportCardId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardTargets_TargetId_BehaviourTargets' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardTargets]'))
BEGIN
ALTER TABLE [dbo].[ReportCardTargets]
    ADD CONSTRAINT [FK_ReportCardTargets_TargetId_BehaviourTargets] FOREIGN KEY ([TargetId]) REFERENCES [dbo].[BehaviourTargets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardTargets_TargetId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardTargets]'))
BEGIN
CREATE INDEX [IX_ReportCardTargets_TargetId] ON [dbo].[ReportCardTargets]([TargetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardTargetEntries_EntryId_ReportCardEntries' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardTargetEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardTargetEntries]
    ADD CONSTRAINT [FK_ReportCardTargetEntries_EntryId_ReportCardEntries] FOREIGN KEY ([EntryId]) REFERENCES [dbo].[ReportCardEntries]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardTargetEntries_EntryId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardTargetEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardTargetEntries_EntryId] ON [dbo].[ReportCardTargetEntries]([EntryId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReportCardTargetEntries_TargetId_ReportCardTargets' AND parent_object_id = OBJECT_ID(N'[dbo].[ReportCardTargetEntries]'))
BEGIN
ALTER TABLE [dbo].[ReportCardTargetEntries]
    ADD CONSTRAINT [FK_ReportCardTargetEntries_TargetId_ReportCardTargets] FOREIGN KEY ([TargetId]) REFERENCES [dbo].[ReportCardTargets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReportCardTargetEntries_TargetId' AND object_id = OBJECT_ID(N'[dbo].[ReportCardTargetEntries]'))
BEGIN
CREATE INDEX [IX_ReportCardTargetEntries_TargetId] ON [dbo].[ReportCardTargetEntries]([TargetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_ResultSetId_ResultSets' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_ResultSetId_ResultSets] FOREIGN KEY ([ResultSetId]) REFERENCES [dbo].[ResultSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_ResultSetId' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_ResultSetId] ON [dbo].[Results]([ResultSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_AspectId_Aspects' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_AspectId_Aspects] FOREIGN KEY ([AspectId]) REFERENCES [dbo].[Aspects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_AspectId' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_AspectId] ON [dbo].[Results]([AspectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_StudentId' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_StudentId] ON [dbo].[Results]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_GradeId_Grades' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_GradeId_Grades] FOREIGN KEY ([GradeId]) REFERENCES [dbo].[Grades]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_GradeId' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_GradeId] ON [dbo].[Results]([GradeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_CreatedById] ON [dbo].[Results]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
CREATE INDEX [IX_Results_LastModifiedById] ON [dbo].[Results]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ResultSetReleases_ResultSetId_ResultSets' AND parent_object_id = OBJECT_ID(N'[dbo].[ResultSetReleases]'))
BEGIN
ALTER TABLE [dbo].[ResultSetReleases]
    ADD CONSTRAINT [FK_ResultSetReleases_ResultSetId_ResultSets] FOREIGN KEY ([ResultSetId]) REFERENCES [dbo].[ResultSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResultSetReleases_ResultSetId' AND object_id = OBJECT_ID(N'[dbo].[ResultSetReleases]'))
BEGIN
CREATE INDEX [IX_ResultSetReleases_ResultSetId] ON [dbo].[ResultSetReleases]([ResultSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ResultSetReleases_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[ResultSetReleases]'))
BEGIN
ALTER TABLE [dbo].[ResultSetReleases]
    ADD CONSTRAINT [FK_ResultSetReleases_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResultSetReleases_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[ResultSetReleases]'))
BEGIN
CREATE INDEX [IX_ResultSetReleases_StudentGroupId] ON [dbo].[ResultSetReleases]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RolePermissions_RoleId_Roles' AND parent_object_id = OBJECT_ID(N'[dbo].[RolePermissions]'))
BEGIN
ALTER TABLE [dbo].[RolePermissions]
    ADD CONSTRAINT [FK_RolePermissions_RoleId_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RolePermissions_RoleId' AND object_id = OBJECT_ID(N'[dbo].[RolePermissions]'))
BEGIN
CREATE INDEX [IX_RolePermissions_RoleId] ON [dbo].[RolePermissions]([RoleId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RolePermissions_PermissionId_Permissions' AND parent_object_id = OBJECT_ID(N'[dbo].[RolePermissions]'))
BEGIN
ALTER TABLE [dbo].[RolePermissions]
    ADD CONSTRAINT [FK_RolePermissions_PermissionId_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RolePermissions_PermissionId' AND object_id = OBJECT_ID(N'[dbo].[RolePermissions]'))
BEGIN
CREATE INDEX [IX_RolePermissions_PermissionId] ON [dbo].[RolePermissions]([PermissionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Rooms_BuildingFloorId_BuildingFloors' AND parent_object_id = OBJECT_ID(N'[dbo].[Rooms]'))
BEGIN
ALTER TABLE [dbo].[Rooms]
    ADD CONSTRAINT [FK_Rooms_BuildingFloorId_BuildingFloors] FOREIGN KEY ([BuildingFloorId]) REFERENCES [dbo].[BuildingFloors]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Rooms_BuildingFloorId' AND object_id = OBJECT_ID(N'[dbo].[Rooms]'))
BEGIN
CREATE INDEX [IX_Rooms_BuildingFloorId] ON [dbo].[Rooms]([BuildingFloorId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RoomClosures_RoomId_Rooms' AND parent_object_id = OBJECT_ID(N'[dbo].[RoomClosures]'))
BEGIN
ALTER TABLE [dbo].[RoomClosures]
    ADD CONSTRAINT [FK_RoomClosures_RoomId_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoomClosures_RoomId' AND object_id = OBJECT_ID(N'[dbo].[RoomClosures]'))
BEGIN
CREATE INDEX [IX_RoomClosures_RoomId] ON [dbo].[RoomClosures]([RoomId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RoomClosures_ReasonId_RoomClosureReasons' AND parent_object_id = OBJECT_ID(N'[dbo].[RoomClosures]'))
BEGIN
ALTER TABLE [dbo].[RoomClosures]
    ADD CONSTRAINT [FK_RoomClosures_ReasonId_RoomClosureReasons] FOREIGN KEY ([ReasonId]) REFERENCES [dbo].[RoomClosureReasons]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoomClosures_ReasonId' AND object_id = OBJECT_ID(N'[dbo].[RoomClosures]'))
BEGIN
CREATE INDEX [IX_RoomClosures_ReasonId] ON [dbo].[RoomClosures]([ReasonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_AgencyId_Agencies' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_AgencyId_Agencies] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_AgencyId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_AgencyId] ON [dbo].[Schools]([AgencyId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_SchoolPhaseId_SchoolPhases' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_SchoolPhaseId_SchoolPhases] FOREIGN KEY ([SchoolPhaseId]) REFERENCES [dbo].[SchoolPhases]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_SchoolPhaseId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_SchoolPhaseId] ON [dbo].[Schools]([SchoolPhaseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_SchoolTypeId_SchoolTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_SchoolTypeId_SchoolTypes] FOREIGN KEY ([SchoolTypeId]) REFERENCES [dbo].[SchoolTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_SchoolTypeId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_SchoolTypeId] ON [dbo].[Schools]([SchoolTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_GovernanceTypeId_GovernanceTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_GovernanceTypeId_GovernanceTypes] FOREIGN KEY ([GovernanceTypeId]) REFERENCES [dbo].[GovernanceTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_GovernanceTypeId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_GovernanceTypeId] ON [dbo].[Schools]([GovernanceTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_IntakeTypeId_IntakeTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_IntakeTypeId_IntakeTypes] FOREIGN KEY ([IntakeTypeId]) REFERENCES [dbo].[IntakeTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_IntakeTypeId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_IntakeTypeId] ON [dbo].[Schools]([IntakeTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_HeadTeacherId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_HeadTeacherId_People] FOREIGN KEY ([HeadTeacherId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_HeadTeacherId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_HeadTeacherId] ON [dbo].[Schools]([HeadTeacherId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_LocalAuthorityId_LocalAuthorities' AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_LocalAuthorityId_LocalAuthorities] FOREIGN KEY ([LocalAuthorityId]) REFERENCES [dbo].[LocalAuthorities]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schools_LocalAuthorityId' AND object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
CREATE INDEX [IX_Schools_LocalAuthorityId] ON [dbo].[Schools]([LocalAuthorityId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenEvents_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[SenEvents]'))
BEGIN
ALTER TABLE [dbo].[SenEvents]
    ADD CONSTRAINT [FK_SenEvents_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenEvents_StudentId' AND object_id = OBJECT_ID(N'[dbo].[SenEvents]'))
BEGIN
CREATE INDEX [IX_SenEvents_StudentId] ON [dbo].[SenEvents]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenEvents_SenEventTypeId_SenEventTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[SenEvents]'))
BEGIN
ALTER TABLE [dbo].[SenEvents]
    ADD CONSTRAINT [FK_SenEvents_SenEventTypeId_SenEventTypes] FOREIGN KEY ([SenEventTypeId]) REFERENCES [dbo].[SenEventTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenEvents_SenEventTypeId' AND object_id = OBJECT_ID(N'[dbo].[SenEvents]'))
BEGIN
CREATE INDEX [IX_SenEvents_SenEventTypeId] ON [dbo].[SenEvents]([SenEventTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenProvisions_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[SenProvisions]'))
BEGIN
ALTER TABLE [dbo].[SenProvisions]
    ADD CONSTRAINT [FK_SenProvisions_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenProvisions_StudentId' AND object_id = OBJECT_ID(N'[dbo].[SenProvisions]'))
BEGIN
CREATE INDEX [IX_SenProvisions_StudentId] ON [dbo].[SenProvisions]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenProvisions_SenProvisionTypeId_SenProvisionTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[SenProvisions]'))
BEGIN
ALTER TABLE [dbo].[SenProvisions]
    ADD CONSTRAINT [FK_SenProvisions_SenProvisionTypeId_SenProvisionTypes] FOREIGN KEY ([SenProvisionTypeId]) REFERENCES [dbo].[SenProvisionTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenProvisions_SenProvisionTypeId' AND object_id = OBJECT_ID(N'[dbo].[SenProvisions]'))
BEGIN
CREATE INDEX [IX_SenProvisions_SenProvisionTypeId] ON [dbo].[SenProvisions]([SenProvisionTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_StudentId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_StudentId] ON [dbo].[SenReviews]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_SencoId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_SencoId_StaffMembers] FOREIGN KEY ([SencoId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_SencoId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_SencoId] ON [dbo].[SenReviews]([SencoId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_DiaryEventId_DiaryEvents' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_DiaryEventId_DiaryEvents] FOREIGN KEY ([DiaryEventId]) REFERENCES [dbo].[DiaryEvents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_DiaryEventId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_DiaryEventId] ON [dbo].[SenReviews]([DiaryEventId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_OutcomeSenStatusId_SenStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_OutcomeSenStatusId_SenStatus] FOREIGN KEY ([OutcomeSenStatusId]) REFERENCES [dbo].[SenStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_OutcomeSenStatusId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_OutcomeSenStatusId] ON [dbo].[SenReviews]([OutcomeSenStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_SenReviewStatusId_SenReviewStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_SenReviewStatusId_SenReviewStatus] FOREIGN KEY ([SenReviewStatusId]) REFERENCES [dbo].[SenReviewStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_SenReviewStatusId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_SenReviewStatusId] ON [dbo].[SenReviews]([SenReviewStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SenReviews_SenReviewTypeId_SenReviewTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
ALTER TABLE [dbo].[SenReviews]
    ADD CONSTRAINT [FK_SenReviews_SenReviewTypeId_SenReviewTypes] FOREIGN KEY ([SenReviewTypeId]) REFERENCES [dbo].[SenReviewTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SenReviews_SenReviewTypeId' AND object_id = OBJECT_ID(N'[dbo].[SenReviews]'))
BEGIN
CREATE INDEX [IX_SenReviews_SenReviewTypeId] ON [dbo].[SenReviews]([SenReviewTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sessions_TeacherId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
ALTER TABLE [dbo].[Sessions]
    ADD CONSTRAINT [FK_Sessions_TeacherId_StaffMembers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sessions_TeacherId' AND object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
CREATE INDEX [IX_Sessions_TeacherId] ON [dbo].[Sessions]([TeacherId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sessions_ClassId_Classes' AND parent_object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
ALTER TABLE [dbo].[Sessions]
    ADD CONSTRAINT [FK_Sessions_ClassId_Classes] FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sessions_ClassId' AND object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
CREATE INDEX [IX_Sessions_ClassId] ON [dbo].[Sessions]([ClassId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sessions_RoomId_Rooms' AND parent_object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
ALTER TABLE [dbo].[Sessions]
    ADD CONSTRAINT [FK_Sessions_RoomId_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sessions_RoomId' AND object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
CREATE INDEX [IX_Sessions_RoomId] ON [dbo].[Sessions]([RoomId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SessionExtraNames_AttendanceWeekId_AttendanceWeeks' AND parent_object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
ALTER TABLE [dbo].[SessionExtraNames]
    ADD CONSTRAINT [FK_SessionExtraNames_AttendanceWeekId_AttendanceWeeks] FOREIGN KEY ([AttendanceWeekId]) REFERENCES [dbo].[AttendanceWeeks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionExtraNames_AttendanceWeekId' AND object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
CREATE INDEX [IX_SessionExtraNames_AttendanceWeekId] ON [dbo].[SessionExtraNames]([AttendanceWeekId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SessionExtraNames_SessionId_Sessions' AND parent_object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
ALTER TABLE [dbo].[SessionExtraNames]
    ADD CONSTRAINT [FK_SessionExtraNames_SessionId_Sessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[Sessions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionExtraNames_SessionId' AND object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
CREATE INDEX [IX_SessionExtraNames_SessionId] ON [dbo].[SessionExtraNames]([SessionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SessionExtraNames_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
ALTER TABLE [dbo].[SessionExtraNames]
    ADD CONSTRAINT [FK_SessionExtraNames_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionExtraNames_StudentId' AND object_id = OBJECT_ID(N'[dbo].[SessionExtraNames]'))
BEGIN
CREATE INDEX [IX_SessionExtraNames_StudentId] ON [dbo].[SessionExtraNames]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SessionPeriods_SessionId_Sessions' AND parent_object_id = OBJECT_ID(N'[dbo].[SessionPeriods]'))
BEGIN
ALTER TABLE [dbo].[SessionPeriods]
    ADD CONSTRAINT [FK_SessionPeriods_SessionId_Sessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[Sessions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionPeriods_SessionId' AND object_id = OBJECT_ID(N'[dbo].[SessionPeriods]'))
BEGIN
CREATE INDEX [IX_SessionPeriods_SessionId] ON [dbo].[SessionPeriods]([SessionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SessionPeriods_PeriodId_AttendancePeriods' AND parent_object_id = OBJECT_ID(N'[dbo].[SessionPeriods]'))
BEGIN
ALTER TABLE [dbo].[SessionPeriods]
    ADD CONSTRAINT [FK_SessionPeriods_PeriodId_AttendancePeriods] FOREIGN KEY ([PeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionPeriods_PeriodId' AND object_id = OBJECT_ID(N'[dbo].[SessionPeriods]'))
BEGIN
CREATE INDEX [IX_SessionPeriods_PeriodId] ON [dbo].[SessionPeriods]([PeriodId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsences_StaffMemberId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
ALTER TABLE [dbo].[StaffAbsences]
    ADD CONSTRAINT [FK_StaffAbsences_StaffMemberId_StaffMembers] FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffAbsences_StaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
CREATE INDEX [IX_StaffAbsences_StaffMemberId] ON [dbo].[StaffAbsences]([StaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsences_AbsenceTypeId_StaffAbsenceTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
ALTER TABLE [dbo].[StaffAbsences]
    ADD CONSTRAINT [FK_StaffAbsences_AbsenceTypeId_StaffAbsenceTypes] FOREIGN KEY ([AbsenceTypeId]) REFERENCES [dbo].[StaffAbsenceTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffAbsences_AbsenceTypeId' AND object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
CREATE INDEX [IX_StaffAbsences_AbsenceTypeId] ON [dbo].[StaffAbsences]([AbsenceTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsences_IllnessTypeId_StaffIllnessTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
ALTER TABLE [dbo].[StaffAbsences]
    ADD CONSTRAINT [FK_StaffAbsences_IllnessTypeId_StaffIllnessTypes] FOREIGN KEY ([IllnessTypeId]) REFERENCES [dbo].[StaffIllnessTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffAbsences_IllnessTypeId' AND object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
BEGIN
CREATE INDEX [IX_StaffAbsences_IllnessTypeId] ON [dbo].[StaffAbsences]([IllnessTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
ALTER TABLE [dbo].[StaffMembers]
    ADD CONSTRAINT [FK_StaffMembers_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffMembers_PersonId' AND object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
CREATE INDEX [IX_StaffMembers_PersonId] ON [dbo].[StaffMembers]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_LineManagerId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
ALTER TABLE [dbo].[StaffMembers]
    ADD CONSTRAINT [FK_StaffMembers_LineManagerId_StaffMembers] FOREIGN KEY ([LineManagerId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffMembers_LineManagerId' AND object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
CREATE INDEX [IX_StaffMembers_LineManagerId] ON [dbo].[StaffMembers]([LineManagerId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StoreDiscounts_ProductId_Products' AND parent_object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
ALTER TABLE [dbo].[StoreDiscounts]
    ADD CONSTRAINT [FK_StoreDiscounts_ProductId_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StoreDiscounts_ProductId' AND object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
CREATE INDEX [IX_StoreDiscounts_ProductId] ON [dbo].[StoreDiscounts]([ProductId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StoreDiscounts_ProductTypeId_ProductTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
ALTER TABLE [dbo].[StoreDiscounts]
    ADD CONSTRAINT [FK_StoreDiscounts_ProductTypeId_ProductTypes] FOREIGN KEY ([ProductTypeId]) REFERENCES [dbo].[ProductTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StoreDiscounts_ProductTypeId' AND object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
CREATE INDEX [IX_StoreDiscounts_ProductTypeId] ON [dbo].[StoreDiscounts]([ProductTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StoreDiscounts_DiscountId_Discounts' AND parent_object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
ALTER TABLE [dbo].[StoreDiscounts]
    ADD CONSTRAINT [FK_StoreDiscounts_DiscountId_Discounts] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discounts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StoreDiscounts_DiscountId' AND object_id = OBJECT_ID(N'[dbo].[StoreDiscounts]'))
BEGIN
CREATE INDEX [IX_StoreDiscounts_DiscountId] ON [dbo].[StoreDiscounts]([DiscountId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
ALTER TABLE [dbo].[Students]
    ADD CONSTRAINT [FK_Students_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_PersonId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
CREATE INDEX [IX_Students_PersonId] ON [dbo].[Students]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_SenStatusId_SenStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
ALTER TABLE [dbo].[Students]
    ADD CONSTRAINT [FK_Students_SenStatusId_SenStatus] FOREIGN KEY ([SenStatusId]) REFERENCES [dbo].[SenStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_SenStatusId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
CREATE INDEX [IX_Students_SenStatusId] ON [dbo].[Students]([SenStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_SenTypeId_SenTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
ALTER TABLE [dbo].[Students]
    ADD CONSTRAINT [FK_Students_SenTypeId_SenTypes] FOREIGN KEY ([SenTypeId]) REFERENCES [dbo].[SenTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_SenTypeId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
CREATE INDEX [IX_Students_SenTypeId] ON [dbo].[Students]([SenTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_EnrolmentStatusId_EnrolmentStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
ALTER TABLE [dbo].[Students]
    ADD CONSTRAINT [FK_Students_EnrolmentStatusId_EnrolmentStatus] FOREIGN KEY ([EnrolmentStatusId]) REFERENCES [dbo].[EnrolmentStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_EnrolmentStatusId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
CREATE INDEX [IX_Students_EnrolmentStatusId] ON [dbo].[Students]([EnrolmentStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_BoarderStatusId_BoarderStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
ALTER TABLE [dbo].[Students]
    ADD CONSTRAINT [FK_Students_BoarderStatusId_BoarderStatus] FOREIGN KEY ([BoarderStatusId]) REFERENCES [dbo].[BoarderStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_BoarderStatusId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
CREATE INDEX [IX_Students_BoarderStatusId] ON [dbo].[Students]([BoarderStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAchievements_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
ALTER TABLE [dbo].[StudentAchievements]
    ADD CONSTRAINT [FK_StudentAchievements_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAchievements_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
CREATE INDEX [IX_StudentAchievements_StudentId] ON [dbo].[StudentAchievements]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAchievements_AchievementId_Achievements' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
ALTER TABLE [dbo].[StudentAchievements]
    ADD CONSTRAINT [FK_StudentAchievements_AchievementId_Achievements] FOREIGN KEY ([AchievementId]) REFERENCES [dbo].[Achievements]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAchievements_AchievementId' AND object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
CREATE INDEX [IX_StudentAchievements_AchievementId] ON [dbo].[StudentAchievements]([AchievementId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAchievements_OutcomeId_AchievementOutcomes' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
ALTER TABLE [dbo].[StudentAchievements]
    ADD CONSTRAINT [FK_StudentAchievements_OutcomeId_AchievementOutcomes] FOREIGN KEY ([OutcomeId]) REFERENCES [dbo].[AchievementOutcomes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAchievements_OutcomeId' AND object_id = OBJECT_ID(N'[dbo].[StudentAchievements]'))
BEGIN
CREATE INDEX [IX_StudentAchievements_OutcomeId] ON [dbo].[StudentAchievements]([OutcomeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAgentRelationships_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentAgentRelationships]
    ADD CONSTRAINT [FK_StudentAgentRelationships_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAgentRelationships_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
CREATE INDEX [IX_StudentAgentRelationships_StudentId] ON [dbo].[StudentAgentRelationships]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAgentRelationships_AgentId_Agents' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentAgentRelationships]
    ADD CONSTRAINT [FK_StudentAgentRelationships_AgentId_Agents] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAgentRelationships_AgentId' AND object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
CREATE INDEX [IX_StudentAgentRelationships_AgentId] ON [dbo].[StudentAgentRelationships]([AgentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentAgentRelationships_RelationshipTypeId_RelationshipTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentAgentRelationships]
    ADD CONSTRAINT [FK_StudentAgentRelationships_RelationshipTypeId_RelationshipTypes] FOREIGN KEY ([RelationshipTypeId]) REFERENCES [dbo].[RelationshipTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentAgentRelationships_RelationshipTypeId' AND object_id = OBJECT_ID(N'[dbo].[StudentAgentRelationships]'))
BEGIN
CREATE INDEX [IX_StudentAgentRelationships_RelationshipTypeId] ON [dbo].[StudentAgentRelationships]([RelationshipTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentCharges_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
ALTER TABLE [dbo].[StudentCharges]
    ADD CONSTRAINT [FK_StudentCharges_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentCharges_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
CREATE INDEX [IX_StudentCharges_StudentId] ON [dbo].[StudentCharges]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentCharges_ChargeId_Charges' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
ALTER TABLE [dbo].[StudentCharges]
    ADD CONSTRAINT [FK_StudentCharges_ChargeId_Charges] FOREIGN KEY ([ChargeId]) REFERENCES [dbo].[Charges]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentCharges_ChargeId' AND object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
CREATE INDEX [IX_StudentCharges_ChargeId] ON [dbo].[StudentCharges]([ChargeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentCharges_ChargeBillingPeriodId_ChargeBillingPeriods' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
ALTER TABLE [dbo].[StudentCharges]
    ADD CONSTRAINT [FK_StudentCharges_ChargeBillingPeriodId_ChargeBillingPeriods] FOREIGN KEY ([ChargeBillingPeriodId]) REFERENCES [dbo].[ChargeBillingPeriods]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentCharges_ChargeBillingPeriodId' AND object_id = OBJECT_ID(N'[dbo].[StudentCharges]'))
BEGIN
CREATE INDEX [IX_StudentCharges_ChargeBillingPeriodId] ON [dbo].[StudentCharges]([ChargeBillingPeriodId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentChargeDiscounts_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentChargeDiscounts]'))
BEGIN
ALTER TABLE [dbo].[StudentChargeDiscounts]
    ADD CONSTRAINT [FK_StudentChargeDiscounts_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentChargeDiscounts_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentChargeDiscounts]'))
BEGIN
CREATE INDEX [IX_StudentChargeDiscounts_StudentId] ON [dbo].[StudentChargeDiscounts]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentChargeDiscounts_ChargeDiscountId_ChargeDiscounts' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentChargeDiscounts]'))
BEGIN
ALTER TABLE [dbo].[StudentChargeDiscounts]
    ADD CONSTRAINT [FK_StudentChargeDiscounts_ChargeDiscountId_ChargeDiscounts] FOREIGN KEY ([ChargeDiscountId]) REFERENCES [dbo].[ChargeDiscounts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentChargeDiscounts_ChargeDiscountId' AND object_id = OBJECT_ID(N'[dbo].[StudentChargeDiscounts]'))
BEGIN
CREATE INDEX [IX_StudentChargeDiscounts_ChargeDiscountId] ON [dbo].[StudentChargeDiscounts]([ChargeDiscountId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentContactRelationships_RelationshipTypeId_RelationshipTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentContactRelationships]
    ADD CONSTRAINT [FK_StudentContactRelationships_RelationshipTypeId_RelationshipTypes] FOREIGN KEY ([RelationshipTypeId]) REFERENCES [dbo].[RelationshipTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentContactRelationships_RelationshipTypeId' AND object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
CREATE INDEX [IX_StudentContactRelationships_RelationshipTypeId] ON [dbo].[StudentContactRelationships]([RelationshipTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentContactRelationships_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentContactRelationships]
    ADD CONSTRAINT [FK_StudentContactRelationships_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentContactRelationships_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
CREATE INDEX [IX_StudentContactRelationships_StudentId] ON [dbo].[StudentContactRelationships]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentContactRelationships_ContactId_Contacts' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
ALTER TABLE [dbo].[StudentContactRelationships]
    ADD CONSTRAINT [FK_StudentContactRelationships_ContactId_Contacts] FOREIGN KEY ([ContactId]) REFERENCES [dbo].[Contacts]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentContactRelationships_ContactId' AND object_id = OBJECT_ID(N'[dbo].[StudentContactRelationships]'))
BEGIN
CREATE INDEX [IX_StudentContactRelationships_ContactId] ON [dbo].[StudentContactRelationships]([ContactId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentDetentions_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
ALTER TABLE [dbo].[StudentDetentions]
    ADD CONSTRAINT [FK_StudentDetentions_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentDetentions_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
CREATE INDEX [IX_StudentDetentions_StudentId] ON [dbo].[StudentDetentions]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentDetentions_DetentionId_Detentions' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
ALTER TABLE [dbo].[StudentDetentions]
    ADD CONSTRAINT [FK_StudentDetentions_DetentionId_Detentions] FOREIGN KEY ([DetentionId]) REFERENCES [dbo].[Detentions]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentDetentions_DetentionId' AND object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
CREATE INDEX [IX_StudentDetentions_DetentionId] ON [dbo].[StudentDetentions]([DetentionId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentDetentions_LinkedIncidentId_StudentIncidents' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
ALTER TABLE [dbo].[StudentDetentions]
    ADD CONSTRAINT [FK_StudentDetentions_LinkedIncidentId_StudentIncidents] FOREIGN KEY ([LinkedIncidentId]) REFERENCES [dbo].[StudentIncidents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentDetentions_LinkedIncidentId' AND object_id = OBJECT_ID(N'[dbo].[StudentDetentions]'))
BEGIN
CREATE INDEX [IX_StudentDetentions_LinkedIncidentId] ON [dbo].[StudentDetentions]([LinkedIncidentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroups_PromoteToGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroups]'))
BEGIN
ALTER TABLE [dbo].[StudentGroups]
    ADD CONSTRAINT [FK_StudentGroups_PromoteToGroupId_StudentGroups] FOREIGN KEY ([PromoteToGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroups_PromoteToGroupId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroups]'))
BEGIN
CREATE INDEX [IX_StudentGroups_PromoteToGroupId] ON [dbo].[StudentGroups]([PromoteToGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroups_MainSupervisorId_StudentGroupSupervisors' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroups]'))
BEGIN
ALTER TABLE [dbo].[StudentGroups]
    ADD CONSTRAINT [FK_StudentGroups_MainSupervisorId_StudentGroupSupervisors] FOREIGN KEY ([MainSupervisorId]) REFERENCES [dbo].[StudentGroupSupervisors]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroups_MainSupervisorId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroups]'))
BEGIN
CREATE INDEX [IX_StudentGroups_MainSupervisorId] ON [dbo].[StudentGroups]([MainSupervisorId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroupMemberships_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroupMemberships]'))
BEGIN
ALTER TABLE [dbo].[StudentGroupMemberships]
    ADD CONSTRAINT [FK_StudentGroupMemberships_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroupMemberships_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroupMemberships]'))
BEGIN
CREATE INDEX [IX_StudentGroupMemberships_StudentId] ON [dbo].[StudentGroupMemberships]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroupMemberships_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroupMemberships]'))
BEGIN
ALTER TABLE [dbo].[StudentGroupMemberships]
    ADD CONSTRAINT [FK_StudentGroupMemberships_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroupMemberships_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroupMemberships]'))
BEGIN
CREATE INDEX [IX_StudentGroupMemberships_StudentGroupId] ON [dbo].[StudentGroupMemberships]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroupSupervisors_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroupSupervisors]'))
BEGIN
ALTER TABLE [dbo].[StudentGroupSupervisors]
    ADD CONSTRAINT [FK_StudentGroupSupervisors_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroupSupervisors_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroupSupervisors]'))
BEGIN
CREATE INDEX [IX_StudentGroupSupervisors_StudentGroupId] ON [dbo].[StudentGroupSupervisors]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentGroupSupervisors_SupervisorId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentGroupSupervisors]'))
BEGIN
ALTER TABLE [dbo].[StudentGroupSupervisors]
    ADD CONSTRAINT [FK_StudentGroupSupervisors_SupervisorId_StaffMembers] FOREIGN KEY ([SupervisorId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentGroupSupervisors_SupervisorId' AND object_id = OBJECT_ID(N'[dbo].[StudentGroupSupervisors]'))
BEGIN
CREATE INDEX [IX_StudentGroupSupervisors_SupervisorId] ON [dbo].[StudentGroupSupervisors]([SupervisorId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentIncidents_StudentId_Students' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
ALTER TABLE [dbo].[StudentIncidents]
    ADD CONSTRAINT [FK_StudentIncidents_StudentId_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentIncidents_StudentId' AND object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
CREATE INDEX [IX_StudentIncidents_StudentId] ON [dbo].[StudentIncidents]([StudentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentIncidents_IncidentId_Incidents' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
ALTER TABLE [dbo].[StudentIncidents]
    ADD CONSTRAINT [FK_StudentIncidents_IncidentId_Incidents] FOREIGN KEY ([IncidentId]) REFERENCES [dbo].[Incidents]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentIncidents_IncidentId' AND object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
CREATE INDEX [IX_StudentIncidents_IncidentId] ON [dbo].[StudentIncidents]([IncidentId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentIncidents_RoleTypeId_BehaviourRoleTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
ALTER TABLE [dbo].[StudentIncidents]
    ADD CONSTRAINT [FK_StudentIncidents_RoleTypeId_BehaviourRoleTypes] FOREIGN KEY ([RoleTypeId]) REFERENCES [dbo].[BehaviourRoleTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentIncidents_RoleTypeId' AND object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
CREATE INDEX [IX_StudentIncidents_RoleTypeId] ON [dbo].[StudentIncidents]([RoleTypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentIncidents_OutcomeId_BehaviourOutcomes' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
ALTER TABLE [dbo].[StudentIncidents]
    ADD CONSTRAINT [FK_StudentIncidents_OutcomeId_BehaviourOutcomes] FOREIGN KEY ([OutcomeId]) REFERENCES [dbo].[BehaviourOutcomes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentIncidents_OutcomeId' AND object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
CREATE INDEX [IX_StudentIncidents_OutcomeId] ON [dbo].[StudentIncidents]([OutcomeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudentIncidents_StatusId_BehaviourStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
ALTER TABLE [dbo].[StudentIncidents]
    ADD CONSTRAINT [FK_StudentIncidents_StatusId_BehaviourStatus] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[BehaviourStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentIncidents_StatusId' AND object_id = OBJECT_ID(N'[dbo].[StudentIncidents]'))
BEGIN
CREATE INDEX [IX_StudentIncidents_StatusId] ON [dbo].[StudentIncidents]([StatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StudyTopics_CourseId_Courses' AND parent_object_id = OBJECT_ID(N'[dbo].[StudyTopics]'))
BEGIN
ALTER TABLE [dbo].[StudyTopics]
    ADD CONSTRAINT [FK_StudyTopics_CourseId_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudyTopics_CourseId' AND object_id = OBJECT_ID(N'[dbo].[StudyTopics]'))
BEGIN
CREATE INDEX [IX_StudyTopics_CourseId] ON [dbo].[StudyTopics]([CourseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Subjects_SubjectCodeId_SubjectCodes' AND parent_object_id = OBJECT_ID(N'[dbo].[Subjects]'))
BEGIN
ALTER TABLE [dbo].[Subjects]
    ADD CONSTRAINT [FK_Subjects_SubjectCodeId_SubjectCodes] FOREIGN KEY ([SubjectCodeId]) REFERENCES [dbo].[SubjectCodes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_SubjectCodeId' AND object_id = OBJECT_ID(N'[dbo].[Subjects]'))
BEGIN
CREATE INDEX [IX_Subjects_SubjectCodeId] ON [dbo].[Subjects]([SubjectCodeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectCodes_SubjectCodeSetId_SubjectCodeSets' AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectCodes]'))
BEGIN
ALTER TABLE [dbo].[SubjectCodes]
    ADD CONSTRAINT [FK_SubjectCodes_SubjectCodeSetId_SubjectCodeSets] FOREIGN KEY ([SubjectCodeSetId]) REFERENCES [dbo].[SubjectCodeSets]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubjectCodes_SubjectCodeSetId' AND object_id = OBJECT_ID(N'[dbo].[SubjectCodes]'))
BEGIN
CREATE INDEX [IX_SubjectCodes_SubjectCodeSetId] ON [dbo].[SubjectCodes]([SubjectCodeSetId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectStaffMembers_SubjectId_Subjects' AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
ALTER TABLE [dbo].[SubjectStaffMembers]
    ADD CONSTRAINT [FK_SubjectStaffMembers_SubjectId_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubjectStaffMembers_SubjectId' AND object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
CREATE INDEX [IX_SubjectStaffMembers_SubjectId] ON [dbo].[SubjectStaffMembers]([SubjectId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectStaffMembers_StaffMemberId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
ALTER TABLE [dbo].[SubjectStaffMembers]
    ADD CONSTRAINT [FK_SubjectStaffMembers_StaffMemberId_StaffMembers] FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubjectStaffMembers_StaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
CREATE INDEX [IX_SubjectStaffMembers_StaffMemberId] ON [dbo].[SubjectStaffMembers]([StaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectStaffMembers_RoleId_SubjectStaffMemberRoles' AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
ALTER TABLE [dbo].[SubjectStaffMembers]
    ADD CONSTRAINT [FK_SubjectStaffMembers_RoleId_SubjectStaffMemberRoles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[SubjectStaffMemberRoles]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubjectStaffMembers_RoleId' AND object_id = OBJECT_ID(N'[dbo].[SubjectStaffMembers]'))
BEGIN
CREATE INDEX [IX_SubjectStaffMembers_RoleId] ON [dbo].[SubjectStaffMembers]([RoleId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_AssignedToId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
ALTER TABLE [dbo].[Tasks]
    ADD CONSTRAINT [FK_Tasks_AssignedToId_People] FOREIGN KEY ([AssignedToId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_AssignedToId' AND object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
CREATE INDEX [IX_Tasks_AssignedToId] ON [dbo].[Tasks]([AssignedToId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_TypeId_TaskTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
ALTER TABLE [dbo].[Tasks]
    ADD CONSTRAINT [FK_Tasks_TypeId_TaskTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[TaskTypes]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_TypeId' AND object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
CREATE INDEX [IX_Tasks_TypeId] ON [dbo].[Tasks]([TypeId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_CreatedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
ALTER TABLE [dbo].[Tasks]
    ADD CONSTRAINT [FK_Tasks_CreatedById_Users] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_CreatedById' AND object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
CREATE INDEX [IX_Tasks_CreatedById] ON [dbo].[Tasks]([CreatedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_LastModifiedById_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
ALTER TABLE [dbo].[Tasks]
    ADD CONSTRAINT [FK_Tasks_LastModifiedById_Users] FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_LastModifiedById' AND object_id = OBJECT_ID(N'[dbo].[Tasks]'))
BEGIN
CREATE INDEX [IX_Tasks_LastModifiedById] ON [dbo].[Tasks]([LastModifiedById]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TaskReminders_TaskId_Tasks' AND parent_object_id = OBJECT_ID(N'[dbo].[TaskReminders]'))
BEGIN
ALTER TABLE [dbo].[TaskReminders]
    ADD CONSTRAINT [FK_TaskReminders_TaskId_Tasks] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Tasks]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TaskReminders_TaskId' AND object_id = OBJECT_ID(N'[dbo].[TaskReminders]'))
BEGIN
CREATE INDEX [IX_TaskReminders_TaskId] ON [dbo].[TaskReminders]([TaskId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingCertificates_StaffMemberId_StaffMembers' AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
ALTER TABLE [dbo].[TrainingCertificates]
    ADD CONSTRAINT [FK_TrainingCertificates_StaffMemberId_StaffMembers] FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingCertificates_StaffMemberId' AND object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
CREATE INDEX [IX_TrainingCertificates_StaffMemberId] ON [dbo].[TrainingCertificates]([StaffMemberId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingCertificates_TrainingCourseId_TrainingCourses' AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
ALTER TABLE [dbo].[TrainingCertificates]
    ADD CONSTRAINT [FK_TrainingCertificates_TrainingCourseId_TrainingCourses] FOREIGN KEY ([TrainingCourseId]) REFERENCES [dbo].[TrainingCourses]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingCertificates_TrainingCourseId' AND object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
CREATE INDEX [IX_TrainingCertificates_TrainingCourseId] ON [dbo].[TrainingCertificates]([TrainingCourseId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingCertificates_TrainingCertificateStatusId_TrainingCertificateStatus' AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
ALTER TABLE [dbo].[TrainingCertificates]
    ADD CONSTRAINT [FK_TrainingCertificates_TrainingCertificateStatusId_TrainingCertificateStatus] FOREIGN KEY ([TrainingCertificateStatusId]) REFERENCES [dbo].[TrainingCertificateStatus]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingCertificates_TrainingCertificateStatusId' AND object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
BEGIN
CREATE INDEX [IX_TrainingCertificates_TrainingCertificateStatusId] ON [dbo].[TrainingCertificates]([TrainingCertificateStatusId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_PersonId_People' AND parent_object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
ALTER TABLE [dbo].[Users]
    ADD CONSTRAINT [FK_Users_PersonId_People] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_PersonId' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
CREATE INDEX [IX_Users_PersonId] ON [dbo].[Users]([PersonId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserReminderSettings_UserId_Users' AND parent_object_id = OBJECT_ID(N'[dbo].[UserReminderSettings]'))
BEGIN
ALTER TABLE [dbo].[UserReminderSettings]
    ADD CONSTRAINT [FK_UserReminderSettings_UserId_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserReminderSettings_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserReminderSettings]'))
BEGIN
CREATE INDEX [IX_UserReminderSettings_UserId] ON [dbo].[UserReminderSettings]([UserId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_YearGroups_StudentGroupId_StudentGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[YearGroups]'))
BEGIN
ALTER TABLE [dbo].[YearGroups]
    ADD CONSTRAINT [FK_YearGroups_StudentGroupId_StudentGroups] FOREIGN KEY ([StudentGroupId]) REFERENCES [dbo].[StudentGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_YearGroups_StudentGroupId' AND object_id = OBJECT_ID(N'[dbo].[YearGroups]'))
BEGIN
CREATE INDEX [IX_YearGroups_StudentGroupId] ON [dbo].[YearGroups]([StudentGroupId]);
END


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_YearGroups_CurriculumYearGroupId_CurriculumYearGroups' AND parent_object_id = OBJECT_ID(N'[dbo].[YearGroups]'))
BEGIN
ALTER TABLE [dbo].[YearGroups]
    ADD CONSTRAINT [FK_YearGroups_CurriculumYearGroupId_CurriculumYearGroups] FOREIGN KEY ([CurriculumYearGroupId]) REFERENCES [dbo].[CurriculumYearGroups]([Id]);
END


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_YearGroups_CurriculumYearGroupId' AND object_id = OBJECT_ID(N'[dbo].[YearGroups]'))
BEGIN
CREATE INDEX [IX_YearGroups_CurriculumYearGroupId] ON [dbo].[YearGroups]([CurriculumYearGroupId]);
END

