-- ============================================================================
-- Staff details entities for the staff profile page.
--
-- Adds the lookup tables + main entities backing the new staff details data
-- model. The shape splits into three layers:
--
--   * Lookups: Departments, Nationalities, MaritalStatuses, InductionStatuses,
--     LeavingReasons, ContractTypes, ServiceTerms, PayScales, DbsCheckTypes,
--     RightToWorkDocumentTypes, QualificationLevels.
--
--   * Staff history: StaffEmployments captures each tenure (start, end,
--     leaving reason) so a returning staff member is modelled as a new
--     employment row rather than a duplicated StaffMember. StaffContracts
--     hangs off an employment and carries the churnable terms (post title,
--     department, FTE, hours, pay scale, salary). Department lives on the
--     contract — "current department" is derived from the active contract.
--
--   * Compliance/HR records: DbsChecks, RightToWorkChecks, StaffQualifications
--     hang directly off StaffMember (not Employment) — they're staff-level
--     facts; "current tenure only" is a UX filter, not a schema concern.
--
-- Also augments People (NationalityId, FirstLanguageId, MaritalStatusId) and
-- StaffMembers (TRN, QTS dates, induction tracking, disability) with the
-- biographical/HR fields the staff details page needs. The existing
-- StaffMembers.Qualifications string is retained for backwards compatibility
-- but new entries should write StaffQualifications rows instead.
-- ============================================================================

-- ============================================================================
-- Lookups
-- ============================================================================

IF OBJECT_ID(N'[dbo].[Departments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Departments] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Departments_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    [ColourCode] nvarchar(9) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_Departments_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[Nationalities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Nationalities] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Nationalities_Active DEFAULT (1),
    [IsoCode] nvarchar(3) NULL,
    CONSTRAINT PK_Nationalities PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[MaritalStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[MaritalStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_MaritalStatuses_Active DEFAULT (1),
    CONSTRAINT PK_MaritalStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[InductionStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[InductionStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_InductionStatuses_Active DEFAULT (1),
    [ColourCode] nvarchar(9) NULL,
    CONSTRAINT PK_InductionStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[LeavingReasons]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LeavingReasons] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_LeavingReasons_Active DEFAULT (1),
    CONSTRAINT PK_LeavingReasons PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[ContractTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ContractTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ContractTypes_Active DEFAULT (1),
    CONSTRAINT PK_ContractTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[ServiceTerms]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ServiceTerms] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ServiceTerms_Active DEFAULT (1),
    CONSTRAINT PK_ServiceTerms PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[PayScales]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PayScales] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_PayScales_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    CONSTRAINT PK_PayScales PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[DbsCheckTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DbsCheckTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_DbsCheckTypes_Active DEFAULT (1),
    CONSTRAINT PK_DbsCheckTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[RightToWorkDocumentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RightToWorkDocumentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_RightToWorkDocumentTypes_Active DEFAULT (1),
    CONSTRAINT PK_RightToWorkDocumentTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[QualificationLevels]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QualificationLevels] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_QualificationLevels_Active DEFAULT (1),
    [OfqualLevel] int NULL,
    CONSTRAINT PK_QualificationLevels PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- Main entities
-- ============================================================================

IF OBJECT_ID(N'[dbo].[StaffEmployments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffEmployments] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [LeavingReasonId] uniqueidentifier NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffEmployments_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffEmployments_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffEmployments_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffEmployments_Version DEFAULT (1),
    CONSTRAINT PK_StaffEmployments PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffContracts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffContracts] (
    [Id] uniqueidentifier NOT NULL,
    [StaffEmploymentId] uniqueidentifier NOT NULL,
    [ContractTypeId] uniqueidentifier NOT NULL,
    [ServiceTermId] uniqueidentifier NULL,
    [DepartmentId] uniqueidentifier NULL,
    [PayScaleId] uniqueidentifier NULL,
    [PostTitle] nvarchar(256) NOT NULL,
    [SpinePoint] nvarchar(20) NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Fte] decimal(5,4) NOT NULL,
    [HoursPerWeek] decimal(5,2) NULL,
    [WeeksPerYear] decimal(4,2) NULL,
    [AnnualSalary] decimal(10,2) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffContracts_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContracts_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContracts_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffContracts_Version DEFAULT (1),
    CONSTRAINT PK_StaffContracts PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[DbsChecks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DbsChecks] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [DbsCheckTypeId] uniqueidentifier NOT NULL,
    [CertificateNumber] nvarchar(20) NOT NULL,
    [IssueDate] datetime2(7) NOT NULL,
    [ExpiryDate] datetime2(7) NULL,
    [UpdateServiceEnrolled] bit NOT NULL CONSTRAINT DF_DbsChecks_UpdateServiceEnrolled DEFAULT (0),
    [LastUpdateServiceCheck] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_DbsChecks_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_DbsChecks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_DbsChecks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_DbsChecks_Version DEFAULT (1),
    CONSTRAINT PK_DbsChecks PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[RightToWorkChecks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RightToWorkChecks] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [DocumentTypeId] uniqueidentifier NOT NULL,
    [VerifiedById] uniqueidentifier NOT NULL,
    [DocumentNumber] nvarchar(64) NULL,
    [CheckDate] datetime2(7) NOT NULL,
    [DocumentExpiryDate] datetime2(7) NULL,
    [FollowUpDate] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_RightToWorkChecks_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_RightToWorkChecks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_RightToWorkChecks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_RightToWorkChecks_Version DEFAULT (1),
    CONSTRAINT PK_RightToWorkChecks PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffQualifications]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffQualifications] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [QualificationLevelId] uniqueidentifier NULL,
    [Title] nvarchar(256) NOT NULL,
    [Subject] nvarchar(256) NULL,
    [AwardingBody] nvarchar(256) NULL,
    [Grade] nvarchar(20) NULL,
    [YearAwarded] int NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffQualifications_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffQualifications_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffQualifications_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffQualifications_Version DEFAULT (1),
    CONSTRAINT PK_StaffQualifications PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- People: biographical additions
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'NationalityId'
)
BEGIN
    ALTER TABLE dbo.People ADD NationalityId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'FirstLanguageId'
)
BEGIN
    ALTER TABLE dbo.People ADD FirstLanguageId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'MaritalStatusId'
)
BEGIN
    ALTER TABLE dbo.People ADD MaritalStatusId uniqueidentifier NULL;
END
GO

-- ============================================================================
-- StaffMembers: HR/employment additions
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'InductionStatusId'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD InductionStatusId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'TeacherReferenceNumber'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD TeacherReferenceNumber nvarchar(7) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'HasQts'
)
BEGIN
    ALTER TABLE dbo.StaffMembers
        ADD HasQts bit NOT NULL
            CONSTRAINT DF_StaffMembers_HasQts DEFAULT (0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'QtsAwardedDate'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD QtsAwardedDate datetime2(7) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'InductionStartDate'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD InductionStartDate datetime2(7) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'InductionCompletedDate'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD InductionCompletedDate datetime2(7) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'HasDisability'
)
BEGIN
    ALTER TABLE dbo.StaffMembers
        ADD HasDisability bit NOT NULL
            CONSTRAINT DF_StaffMembers_HasDisability DEFAULT (0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'DisabilityDetails'
)
BEGIN
    ALTER TABLE dbo.StaffMembers ADD DisabilityDetails nvarchar(max) NULL;
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

-- StaffEmployments ----------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffEmployments_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffEmployments]'))
BEGIN
ALTER TABLE [dbo].[StaffEmployments]
    ADD CONSTRAINT [FK_StaffEmployments_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffEmployments_LeavingReasonId_LeavingReasons'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffEmployments]'))
BEGIN
ALTER TABLE [dbo].[StaffEmployments]
    ADD CONSTRAINT [FK_StaffEmployments_LeavingReasonId_LeavingReasons]
    FOREIGN KEY ([LeavingReasonId]) REFERENCES [dbo].[LeavingReasons]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffEmployments_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffEmployments]'))
BEGIN
ALTER TABLE [dbo].[StaffEmployments]
    ADD CONSTRAINT [FK_StaffEmployments_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffEmployments_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffEmployments]'))
BEGIN
ALTER TABLE [dbo].[StaffEmployments]
    ADD CONSTRAINT [FK_StaffEmployments_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- StaffContracts ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_StaffEmploymentId_StaffEmployments'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_StaffEmploymentId_StaffEmployments]
    FOREIGN KEY ([StaffEmploymentId]) REFERENCES [dbo].[StaffEmployments]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_ContractTypeId_ContractTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_ContractTypeId_ContractTypes]
    FOREIGN KEY ([ContractTypeId]) REFERENCES [dbo].[ContractTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_ServiceTermId_ServiceTerms]
    FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_DepartmentId_Departments'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_DepartmentId_Departments]
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_PayScaleId_PayScales'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_PayScaleId_PayScales]
    FOREIGN KEY ([PayScaleId]) REFERENCES [dbo].[PayScales]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- DbsChecks -----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DbsChecks_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[DbsChecks]'))
BEGIN
ALTER TABLE [dbo].[DbsChecks]
    ADD CONSTRAINT [FK_DbsChecks_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DbsChecks_DbsCheckTypeId_DbsCheckTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[DbsChecks]'))
BEGIN
ALTER TABLE [dbo].[DbsChecks]
    ADD CONSTRAINT [FK_DbsChecks_DbsCheckTypeId_DbsCheckTypes]
    FOREIGN KEY ([DbsCheckTypeId]) REFERENCES [dbo].[DbsCheckTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DbsChecks_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[DbsChecks]'))
BEGIN
ALTER TABLE [dbo].[DbsChecks]
    ADD CONSTRAINT [FK_DbsChecks_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DbsChecks_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[DbsChecks]'))
BEGIN
ALTER TABLE [dbo].[DbsChecks]
    ADD CONSTRAINT [FK_DbsChecks_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- RightToWorkChecks ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RightToWorkChecks_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
ALTER TABLE [dbo].[RightToWorkChecks]
    ADD CONSTRAINT [FK_RightToWorkChecks_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RightToWorkChecks_DocumentTypeId_RightToWorkDocumentTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
ALTER TABLE [dbo].[RightToWorkChecks]
    ADD CONSTRAINT [FK_RightToWorkChecks_DocumentTypeId_RightToWorkDocumentTypes]
    FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[RightToWorkDocumentTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RightToWorkChecks_VerifiedById_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
ALTER TABLE [dbo].[RightToWorkChecks]
    ADD CONSTRAINT [FK_RightToWorkChecks_VerifiedById_StaffMembers]
    FOREIGN KEY ([VerifiedById]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RightToWorkChecks_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
ALTER TABLE [dbo].[RightToWorkChecks]
    ADD CONSTRAINT [FK_RightToWorkChecks_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RightToWorkChecks_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
ALTER TABLE [dbo].[RightToWorkChecks]
    ADD CONSTRAINT [FK_RightToWorkChecks_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- StaffQualifications -------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffQualifications_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffQualifications]'))
BEGIN
ALTER TABLE [dbo].[StaffQualifications]
    ADD CONSTRAINT [FK_StaffQualifications_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffQualifications_QualificationLevelId_QualificationLevels'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffQualifications]'))
BEGIN
ALTER TABLE [dbo].[StaffQualifications]
    ADD CONSTRAINT [FK_StaffQualifications_QualificationLevelId_QualificationLevels]
    FOREIGN KEY ([QualificationLevelId]) REFERENCES [dbo].[QualificationLevels]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffQualifications_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffQualifications]'))
BEGIN
ALTER TABLE [dbo].[StaffQualifications]
    ADD CONSTRAINT [FK_StaffQualifications_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffQualifications_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffQualifications]'))
BEGIN
ALTER TABLE [dbo].[StaffQualifications]
    ADD CONSTRAINT [FK_StaffQualifications_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- People --------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_NationalityId_Nationalities'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_NationalityId_Nationalities]
    FOREIGN KEY ([NationalityId]) REFERENCES [dbo].[Nationalities]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_FirstLanguageId_Languages'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_FirstLanguageId_Languages]
    FOREIGN KEY ([FirstLanguageId]) REFERENCES [dbo].[Languages]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_MaritalStatusId_MaritalStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_MaritalStatusId_MaritalStatuses]
    FOREIGN KEY ([MaritalStatusId]) REFERENCES [dbo].[MaritalStatuses]([Id]);
END
GO

-- StaffMembers --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_InductionStatusId_InductionStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
ALTER TABLE [dbo].[StaffMembers]
    ADD CONSTRAINT [FK_StaffMembers_InductionStatusId_InductionStatuses]
    FOREIGN KEY ([InductionStatusId]) REFERENCES [dbo].[InductionStatuses]([Id]);
END
GO

-- ============================================================================
-- Indexes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffEmployments_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffEmployments]'))
BEGIN
CREATE INDEX [IX_StaffEmployments_StaffMemberId]
    ON [dbo].[StaffEmployments]([StaffMemberId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContracts_StaffEmploymentId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
CREATE INDEX [IX_StaffContracts_StaffEmploymentId]
    ON [dbo].[StaffContracts]([StaffEmploymentId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DbsChecks_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[DbsChecks]'))
BEGIN
CREATE INDEX [IX_DbsChecks_StaffMemberId]
    ON [dbo].[DbsChecks]([StaffMemberId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RightToWorkChecks_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[RightToWorkChecks]'))
BEGIN
CREATE INDEX [IX_RightToWorkChecks_StaffMemberId]
    ON [dbo].[RightToWorkChecks]([StaffMemberId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffQualifications_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffQualifications]'))
BEGIN
CREATE INDEX [IX_StaffQualifications_StaffMemberId]
    ON [dbo].[StaffQualifications]([StaffMemberId]);
END
GO
