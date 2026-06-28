-- ============================================================================
-- Performance (appraisal) area.
--
-- Reuses existing tables where they exist:
--   * Observations / ObservationOutcomes — lesson observations + their grade scale
--     (already present and seeded). Observations gains focus / subject / strengths /
--     areas-for-development columns here. The overall appraisal rating also reuses
--     ObservationOutcomes rather than a parallel lookup.
--   * TrainingCertificates / TrainingCourses / TrainingCertificateStatus — CPD
--     records. TrainingCertificates gains completion / expiry / provider / hours /
--     reference columns, and TrainingCourses gets a starter seed.
--
-- New for this slice:
--   * PerformanceReviews — appraisal review cycles (1:many — one per cycle, so the
--     year-on-year history is preserved). Holds reviewer, status, dates, rating, notes.
--   * StaffObjectives — appraisal objectives / targets (1:many), optionally tied to a
--     review cycle and categorised.
--   * ObjectiveStatuses / ObjectiveCategories / ReviewStatuses lookups (+ seeds).
--
-- Idempotent throughout (guarded CREATE/ALTER, MERGE seeds).
-- ============================================================================

-- ============================================================================
-- Lookups
-- ============================================================================

IF OBJECT_ID(N'[dbo].[ObjectiveStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ObjectiveStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ObjectiveStatuses_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ObjectiveStatuses_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ObjectiveStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[ObjectiveCategories]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ObjectiveCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ObjectiveCategories_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ObjectiveCategories_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ObjectiveCategories PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[ReviewStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReviewStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ReviewStatuses_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ReviewStatuses_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ReviewStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- Observations: enrich with lesson focus / subject / strengths / development
-- ============================================================================

IF COL_LENGTH(N'[dbo].[Observations]', N'Focus') IS NULL
    ALTER TABLE [dbo].[Observations] ADD [Focus] nvarchar(128) NULL;
GO
IF COL_LENGTH(N'[dbo].[Observations]', N'SubjectObserved') IS NULL
    ALTER TABLE [dbo].[Observations] ADD [SubjectObserved] nvarchar(128) NULL;
GO
IF COL_LENGTH(N'[dbo].[Observations]', N'Strengths') IS NULL
    ALTER TABLE [dbo].[Observations] ADD [Strengths] nvarchar(max) NULL;
GO
IF COL_LENGTH(N'[dbo].[Observations]', N'AreasForDevelopment') IS NULL
    ALTER TABLE [dbo].[Observations] ADD [AreasForDevelopment] nvarchar(max) NULL;
GO

-- ============================================================================
-- CPD: completion / expiry / provider / hours / reference
-- ============================================================================

IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'CompletedDate') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [CompletedDate] datetime2(7) NULL;
GO
IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'ExpiryDate') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [ExpiryDate] datetime2(7) NULL;
GO
IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'Provider') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [Provider] nvarchar(128) NULL;
GO
IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'Hours') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [Hours] decimal(6,2) NULL;
GO
IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'CertificateReference') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [CertificateReference] nvarchar(64) NULL;
GO

-- ============================================================================
-- Main entities
-- ============================================================================

IF OBJECT_ID(N'[dbo].[PerformanceReviews]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PerformanceReviews] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [CycleName] nvarchar(128) NULL,
    [ReviewerId] uniqueidentifier NULL,
    [StatusId] uniqueidentifier NULL,
    [ReviewDate] datetime2(7) NULL,
    [NextReviewDate] datetime2(7) NULL,
    [OverallOutcomeId] uniqueidentifier NULL,
    [Summary] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_PerformanceReviews_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_PerformanceReviews_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_PerformanceReviews_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_PerformanceReviews_Version DEFAULT (1),
    CONSTRAINT PK_PerformanceReviews PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffObjectives]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffObjectives] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [ReviewId] uniqueidentifier NULL,
    [CategoryId] uniqueidentifier NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(max) NULL,
    [SuccessCriteria] nvarchar(max) NULL,
    [DueDate] datetime2(7) NULL,
    [StatusId] uniqueidentifier NULL,
    [ProgressNotes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffObjectives_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffObjectives_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffObjectives_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffObjectives_Version DEFAULT (1),
    CONSTRAINT PK_StaffObjectives PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- StaffObjectives may predate this slice (an earlier revision of this script created
-- it without the review-cycle / category / success-criteria columns). Patch them in
-- so a re-run brings an existing table up to the current shape.
IF COL_LENGTH(N'[dbo].[StaffObjectives]', N'ReviewId') IS NULL
    ALTER TABLE [dbo].[StaffObjectives] ADD [ReviewId] uniqueidentifier NULL;
GO
IF COL_LENGTH(N'[dbo].[StaffObjectives]', N'CategoryId') IS NULL
    ALTER TABLE [dbo].[StaffObjectives] ADD [CategoryId] uniqueidentifier NULL;
GO
IF COL_LENGTH(N'[dbo].[StaffObjectives]', N'SuccessCriteria') IS NULL
    ALTER TABLE [dbo].[StaffObjectives] ADD [SuccessCriteria] nvarchar(max) NULL;
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_ReviewerId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_ReviewerId_StaffMembers]
    FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_StatusId_ReviewStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_StatusId_ReviewStatuses]
    FOREIGN KEY ([StatusId]) REFERENCES [dbo].[ReviewStatuses]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_OverallOutcomeId_ObservationOutcomes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_OverallOutcomeId_ObservationOutcomes]
    FOREIGN KEY ([OverallOutcomeId]) REFERENCES [dbo].[ObservationOutcomes]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PerformanceReviews_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
ALTER TABLE [dbo].[PerformanceReviews]
    ADD CONSTRAINT [FK_PerformanceReviews_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_ReviewId_PerformanceReviews'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_ReviewId_PerformanceReviews]
    FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[PerformanceReviews]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_CategoryId_ObjectiveCategories'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_CategoryId_ObjectiveCategories]
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ObjectiveCategories]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_StatusId_ObjectiveStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_StatusId_ObjectiveStatuses]
    FOREIGN KEY ([StatusId]) REFERENCES [dbo].[ObjectiveStatuses]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffObjectives_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
ALTER TABLE [dbo].[StaffObjectives]
    ADD CONSTRAINT [FK_StaffObjectives_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

-- ============================================================================
-- Indexes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PerformanceReviews_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[PerformanceReviews]'))
CREATE INDEX [IX_PerformanceReviews_StaffMemberId] ON [dbo].[PerformanceReviews]([StaffMemberId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffObjectives_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffObjectives]'))
CREATE INDEX [IX_StaffObjectives_StaffMemberId] ON [dbo].[StaffObjectives]([StaffMemberId]);
GO

-- ============================================================================
-- Lookup seeds
-- ============================================================================

-- ObjectiveStatuses ----------------------------------------------------------
MERGE INTO [dbo].[ObjectiveStatuses] AS Target
    USING (VALUES
    ('A7B6C5D4-0001-4000-8000-000000000001', 'Not Started',  'NS',  10),
    ('A7B6C5D4-0001-4000-8000-000000000002', 'In Progress',  'IP',  20),
    ('A7B6C5D4-0001-4000-8000-000000000003', 'Achieved',     'ACH', 30),
    ('A7B6C5D4-0001-4000-8000-000000000004', 'Partially Met','PM',  40),
    ('A7B6C5D4-0001-4000-8000-000000000005', 'Not Met',      'NM',  50),
    ('A7B6C5D4-0001-4000-8000-000000000006', 'Deferred',     'DEF', 60)
    )
    AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder;
GO

-- ObjectiveCategories --------------------------------------------------------
MERGE INTO [dbo].[ObjectiveCategories] AS Target
    USING (VALUES
    ('A7B6C5D4-0002-4000-8000-000000000001', 'Pupil Progress',           'PP',  10),
    ('A7B6C5D4-0002-4000-8000-000000000002', 'Professional Development', 'PD',  20),
    ('A7B6C5D4-0002-4000-8000-000000000003', 'Whole-School',             'WS',  30),
    ('A7B6C5D4-0002-4000-8000-000000000004', 'Leadership & Management',  'LM',  40)
    )
    AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder;
GO

-- ReviewStatuses -------------------------------------------------------------
MERGE INTO [dbo].[ReviewStatuses] AS Target
    USING (VALUES
    ('A7B6C5D4-0003-4000-8000-000000000001', 'Draft',       'DR',  10),
    ('A7B6C5D4-0003-4000-8000-000000000002', 'In Progress', 'IP',  20),
    ('A7B6C5D4-0003-4000-8000-000000000003', 'Completed',   'COM', 30)
    )
    AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder;
GO

-- TrainingCourses (starter set of common compliance courses) ------------------
-- LookupEntity.Description carries the display name; Code/Name are the entity's own fields.
MERGE INTO [dbo].[TrainingCourses] AS Target
    USING (VALUES
    ('B8C7D6E5-0001-4000-8000-000000000001', 'Safeguarding (KCSIE)',    'SG'),
    ('B8C7D6E5-0001-4000-8000-000000000002', 'Prevent Duty',            'PREV'),
    ('B8C7D6E5-0001-4000-8000-000000000003', 'First Aid',               'FA'),
    ('B8C7D6E5-0001-4000-8000-000000000004', 'Fire Safety',             'FIRE'),
    ('B8C7D6E5-0001-4000-8000-000000000005', 'Health & Safety',         'HS'),
    ('B8C7D6E5-0001-4000-8000-000000000006', 'Data Protection (GDPR)',  'GDPR'),
    ('B8C7D6E5-0001-4000-8000-000000000007', 'Manual Handling',         'MH'),
    ('B8C7D6E5-0001-4000-8000-000000000008', 'Equality & Diversity',    'ED')
    )
    AS Source (Id, Name, Code)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Name, Code, Active) VALUES (Id, Name, Name, Code, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Name, Name = Source.Name, Code = Source.Code;
GO
