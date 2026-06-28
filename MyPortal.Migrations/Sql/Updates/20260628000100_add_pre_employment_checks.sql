-- ============================================================================
-- Pre-Employment Checks (Single Central Record) area.
--
-- The DBS (DbsChecks/DbsCheckTypes) and right-to-work (RightToWorkChecks/
-- RightToWorkDocumentTypes) tables already exist from the staff-details
-- migration. This adds the remaining pieces wired up by the new slice:
--
--   * StaffPreEmploymentChecks — 1:1 summary record holding the SCR "checked on
--     this date" flags that don't warrant their own list (identity, prohibition
--     from teaching, s128 management prohibition, childcare disqualification,
--     medical fitness, qualifications verified).
--   * StaffReferences — pre-employment references sought (1:many).
--   * StaffOverseasChecks — overseas conduct checks per country (1:many; the
--     country reuses the Nationalities lookup).
--   * ReferenceTypes / ReferenceStatuses lookups (+ seeds).
--
-- Idempotent throughout (guarded CREATE/ALTER, MERGE seeds).
-- ============================================================================

-- ============================================================================
-- Lookups
-- ============================================================================

IF OBJECT_ID(N'[dbo].[ReferenceTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReferenceTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ReferenceTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ReferenceTypes_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ReferenceTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[ReferenceStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReferenceStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ReferenceStatuses_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ReferenceStatuses_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ReferenceStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- RightToWorkChecks.VerifiedById relaxed to NULL: the verifier is recorded as the
-- current user only where they are themselves a staff member, so it must be optional.
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.RightToWorkChecks') AND name = N'VerifiedById' AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[RightToWorkChecks] ALTER COLUMN [VerifiedById] uniqueidentifier NULL;
END
GO

-- ============================================================================
-- Main entities
-- ============================================================================

IF OBJECT_ID(N'[dbo].[StaffPreEmploymentChecks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffPreEmploymentChecks] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [IdentityCheckedDate] datetime2(7) NULL,
    [ProhibitionFromTeachingCheckedDate] datetime2(7) NULL,
    [ProhibitionFromManagementCheckedDate] datetime2(7) NULL,
    [ChildcareDisqualificationCheckedDate] datetime2(7) NULL,
    [MedicalFitnessCheckedDate] datetime2(7) NULL,
    [QualificationsVerifiedDate] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffPreEmploymentChecks_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffPreEmploymentChecks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffPreEmploymentChecks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffPreEmploymentChecks_Version DEFAULT (1),
    CONSTRAINT PK_StaffPreEmploymentChecks PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffReferences]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffReferences] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [ReferenceTypeId] uniqueidentifier NULL,
    [ReferenceStatusId] uniqueidentifier NULL,
    [RefereeName] nvarchar(256) NOT NULL,
    [RefereeOrganisation] nvarchar(256) NULL,
    [RefereeEmail] nvarchar(256) NULL,
    [RequestedDate] datetime2(7) NULL,
    [ReceivedDate] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffReferences_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffReferences_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffReferences_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffReferences_Version DEFAULT (1),
    CONSTRAINT PK_StaffReferences PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffOverseasChecks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffOverseasChecks] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [NationalityId] uniqueidentifier NOT NULL,
    [CheckedDate] datetime2(7) NULL,
    [IsClear] bit NOT NULL CONSTRAINT DF_StaffOverseasChecks_IsClear DEFAULT (0),
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffOverseasChecks_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffOverseasChecks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffOverseasChecks_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffOverseasChecks_Version DEFAULT (1),
    CONSTRAINT PK_StaffOverseasChecks PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffPreEmploymentChecks_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffPreEmploymentChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffPreEmploymentChecks]
    ADD CONSTRAINT [FK_StaffPreEmploymentChecks_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffPreEmploymentChecks_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffPreEmploymentChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffPreEmploymentChecks]
    ADD CONSTRAINT [FK_StaffPreEmploymentChecks_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffPreEmploymentChecks_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffPreEmploymentChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffPreEmploymentChecks]
    ADD CONSTRAINT [FK_StaffPreEmploymentChecks_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffReferences_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
ALTER TABLE [dbo].[StaffReferences]
    ADD CONSTRAINT [FK_StaffReferences_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffReferences_ReferenceTypeId_ReferenceTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
ALTER TABLE [dbo].[StaffReferences]
    ADD CONSTRAINT [FK_StaffReferences_ReferenceTypeId_ReferenceTypes]
    FOREIGN KEY ([ReferenceTypeId]) REFERENCES [dbo].[ReferenceTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffReferences_ReferenceStatusId_ReferenceStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
ALTER TABLE [dbo].[StaffReferences]
    ADD CONSTRAINT [FK_StaffReferences_ReferenceStatusId_ReferenceStatuses]
    FOREIGN KEY ([ReferenceStatusId]) REFERENCES [dbo].[ReferenceStatuses]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffReferences_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
ALTER TABLE [dbo].[StaffReferences]
    ADD CONSTRAINT [FK_StaffReferences_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffReferences_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
ALTER TABLE [dbo].[StaffReferences]
    ADD CONSTRAINT [FK_StaffReferences_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffOverseasChecks_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffOverseasChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffOverseasChecks]
    ADD CONSTRAINT [FK_StaffOverseasChecks_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffOverseasChecks_NationalityId_Nationalities'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffOverseasChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffOverseasChecks]
    ADD CONSTRAINT [FK_StaffOverseasChecks_NationalityId_Nationalities]
    FOREIGN KEY ([NationalityId]) REFERENCES [dbo].[Nationalities]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffOverseasChecks_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffOverseasChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffOverseasChecks]
    ADD CONSTRAINT [FK_StaffOverseasChecks_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffOverseasChecks_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffOverseasChecks]'))
BEGIN
ALTER TABLE [dbo].[StaffOverseasChecks]
    ADD CONSTRAINT [FK_StaffOverseasChecks_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- ============================================================================
-- Indexes (1:1 record gets a unique index on the staff member)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_StaffPreEmploymentChecks_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffPreEmploymentChecks]'))
BEGIN
CREATE UNIQUE INDEX [UX_StaffPreEmploymentChecks_StaffMemberId]
    ON [dbo].[StaffPreEmploymentChecks]([StaffMemberId]) WHERE [IsDeleted] = 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffReferences_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffReferences]'))
BEGIN
CREATE INDEX [IX_StaffReferences_StaffMemberId]
    ON [dbo].[StaffReferences]([StaffMemberId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffOverseasChecks_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffOverseasChecks]'))
BEGIN
CREATE INDEX [IX_StaffOverseasChecks_StaffMemberId]
    ON [dbo].[StaffOverseasChecks]([StaffMemberId]);
END
GO

-- ============================================================================
-- Lookup seeds
-- ============================================================================

-- ReferenceTypes -------------------------------------------------------------
MERGE INTO [dbo].[ReferenceTypes] AS Target
    USING (VALUES
    ('E1F2A3B4-0001-4000-8000-000000000001', 'Employment', 'EMP', 10),
    ('E1F2A3B4-0001-4000-8000-000000000002', 'Character',  'CHR', 20),
    ('E1F2A3B4-0001-4000-8000-000000000003', 'Academic',   'ACA', 30)
    )
    AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, DisplayOrder, Active)
    VALUES (Id, Description, Code, DisplayOrder, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               Code = Source.Code,
               DisplayOrder = Source.DisplayOrder;
GO

-- ReferenceStatuses ----------------------------------------------------------
MERGE INTO [dbo].[ReferenceStatuses] AS Target
    USING (VALUES
    ('E1F2A3B4-0002-4000-8000-000000000001', 'Requested',    'REQ', 10),
    ('E1F2A3B4-0002-4000-8000-000000000002', 'Received',     'RCV', 20),
    ('E1F2A3B4-0002-4000-8000-000000000003', 'Satisfactory', 'SAT', 30),
    ('E1F2A3B4-0002-4000-8000-000000000004', 'Concerns',     'CON', 40),
    ('E1F2A3B4-0002-4000-8000-000000000005', 'Not Required', 'NA',  50)
    )
    AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, DisplayOrder, Active)
    VALUES (Id, Description, Code, DisplayOrder, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               Code = Source.Code,
               DisplayOrder = Source.DisplayOrder;
GO
