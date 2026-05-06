-- ============================================================================
-- Timetable run/review/apply workflow + per-staff PPA allocation.
-- ============================================================================

-- Timetables -----------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[Timetables]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Timetables] (
    [Id] uniqueidentifier NOT NULL,
    [AcademicYearId] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    -- 0 Draft, 1 Active, 2 Superseded, 3 Discarded.
    [Status] int NOT NULL,
    -- Date window during which this timetable is/was effective. NULLs while in
    -- Draft; populated on apply.
    [EffectiveFrom] date NULL,
    [EffectiveTo] date NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL,
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL,
    [Version] BIGINT NOT NULL CONSTRAINT DF_Timetables_Version DEFAULT (1),
    CONSTRAINT PK_Timetables PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- TimetableRuns --------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[TimetableRuns]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TimetableRuns] (
    [Id] uniqueidentifier NOT NULL,
    [TimetableId] uniqueidentifier NOT NULL,
    -- 0 Running, 1 Succeeded, 2 Failed.
    [Status] int NOT NULL,
    [StartedAt] datetime2(7) NOT NULL,
    [CompletedAt] datetime2(7) NULL,
    [SolverDiagnostic] nvarchar(max) NULL,
    [InputSnapshot] nvarchar(max) NULL,
    [TriggeredById] uniqueidentifier NOT NULL,
    CONSTRAINT PK_TimetableRuns PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- TimetableAssignments -------------------------------------------------------
IF OBJECT_ID(N'[dbo].[TimetableAssignments]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TimetableAssignments] (
    [Id] uniqueidentifier NOT NULL,
    [TimetableId] uniqueidentifier NOT NULL,
    [CurriculumBlockId] uniqueidentifier NOT NULL,
    [SlotIndex] int NOT NULL,
    [ClassId] uniqueidentifier NOT NULL,
    [TeacherId] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NULL,
    [StartAttendancePeriodId] uniqueidentifier NOT NULL,
    [Size] int NOT NULL,
    CONSTRAINT PK_TimetableAssignments PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- TimetablePins --------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[TimetablePins]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TimetablePins] (
    [Id] uniqueidentifier NOT NULL,
    [TimetableId] uniqueidentifier NOT NULL,
    [CurriculumBlockId] uniqueidentifier NOT NULL,
    [SlotIndex] int NOT NULL,
    -- ClassId NULL = pin only the slot's start period (block-level).
    [ClassId] uniqueidentifier NULL,
    [TeacherId] uniqueidentifier NULL,
    [RoomId] uniqueidentifier NULL,
    [StartAttendancePeriodId] uniqueidentifier NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL,
    CONSTRAINT PK_TimetablePins PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- Sessions.TimetableId (provenance, nullable for pre-existing sessions) ------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Sessions') AND name = N'TimetableId'
)
BEGIN
    ALTER TABLE dbo.Sessions ADD TimetableId uniqueidentifier NULL;
END
GO

-- StaffMembers.PpaPeriodsPerWeek --------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'PpaPeriodsPerWeek'
)
BEGIN
    ALTER TABLE dbo.StaffMembers
        ADD PpaPeriodsPerWeek int NOT NULL
            CONSTRAINT DF_StaffMembers_PpaPeriodsPerWeek DEFAULT (0);
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Timetables_AcademicYearId_AcademicYears'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Timetables]'))
BEGIN
ALTER TABLE [dbo].[Timetables]
    ADD CONSTRAINT [FK_Timetables_AcademicYearId_AcademicYears]
    FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Timetables_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Timetables]'))
BEGIN
ALTER TABLE [dbo].[Timetables]
    ADD CONSTRAINT [FK_Timetables_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Timetables_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Timetables]'))
BEGIN
ALTER TABLE [dbo].[Timetables]
    ADD CONSTRAINT [FK_Timetables_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableRuns_TimetableId_Timetables'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableRuns]'))
BEGIN
ALTER TABLE [dbo].[TimetableRuns]
    ADD CONSTRAINT [FK_TimetableRuns_TimetableId_Timetables]
    FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetables]([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableRuns_TriggeredById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableRuns]'))
BEGIN
ALTER TABLE [dbo].[TimetableRuns]
    ADD CONSTRAINT [FK_TimetableRuns_TriggeredById_Users]
    FOREIGN KEY ([TriggeredById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_TimetableId_Timetables'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_TimetableId_Timetables]
    FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetables]([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_CurriculumBlockId_CurriculumBlocks'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_CurriculumBlockId_CurriculumBlocks]
    FOREIGN KEY ([CurriculumBlockId]) REFERENCES [dbo].[CurriculumBlocks]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_ClassId_Classes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_ClassId_Classes]
    FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_TeacherId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_TeacherId_StaffMembers]
    FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_RoomId_Rooms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_RoomId_Rooms]
    FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetableAssignments_StartAttendancePeriodId_AttendancePeriods'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
BEGIN
ALTER TABLE [dbo].[TimetableAssignments]
    ADD CONSTRAINT [FK_TimetableAssignments_StartAttendancePeriodId_AttendancePeriods]
    FOREIGN KEY ([StartAttendancePeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_TimetableId_Timetables'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_TimetableId_Timetables]
    FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetables]([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_CurriculumBlockId_CurriculumBlocks'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_CurriculumBlockId_CurriculumBlocks]
    FOREIGN KEY ([CurriculumBlockId]) REFERENCES [dbo].[CurriculumBlocks]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_ClassId_Classes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_ClassId_Classes]
    FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_TeacherId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_TeacherId_StaffMembers]
    FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_RoomId_Rooms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_RoomId_Rooms]
    FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_StartAttendancePeriodId_AttendancePeriods'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_StartAttendancePeriodId_AttendancePeriods]
    FOREIGN KEY ([StartAttendancePeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TimetablePins_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
BEGIN
ALTER TABLE [dbo].[TimetablePins]
    ADD CONSTRAINT [FK_TimetablePins_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sessions_TimetableId_Timetables'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Sessions]'))
BEGIN
ALTER TABLE [dbo].[Sessions]
    ADD CONSTRAINT [FK_Sessions_TimetableId_Timetables]
    FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetables]([Id]);
END
GO

-- ============================================================================
-- Indexes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Timetables_AcademicYearId'
    AND object_id = OBJECT_ID(N'[dbo].[Timetables]'))
CREATE INDEX [IX_Timetables_AcademicYearId] ON [dbo].[Timetables]([AcademicYearId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Timetables_Status'
    AND object_id = OBJECT_ID(N'[dbo].[Timetables]'))
CREATE INDEX [IX_Timetables_Status] ON [dbo].[Timetables]([Status]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TimetableRuns_TimetableId'
    AND object_id = OBJECT_ID(N'[dbo].[TimetableRuns]'))
CREATE INDEX [IX_TimetableRuns_TimetableId] ON [dbo].[TimetableRuns]([TimetableId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TimetableAssignments_TimetableId'
    AND object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
CREATE INDEX [IX_TimetableAssignments_TimetableId] ON [dbo].[TimetableAssignments]([TimetableId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TimetableAssignments_ClassId'
    AND object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
CREATE INDEX [IX_TimetableAssignments_ClassId] ON [dbo].[TimetableAssignments]([ClassId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TimetableAssignments_TeacherId'
    AND object_id = OBJECT_ID(N'[dbo].[TimetableAssignments]'))
CREATE INDEX [IX_TimetableAssignments_TeacherId] ON [dbo].[TimetableAssignments]([TeacherId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TimetablePins_TimetableId'
    AND object_id = OBJECT_ID(N'[dbo].[TimetablePins]'))
CREATE INDEX [IX_TimetablePins_TimetableId] ON [dbo].[TimetablePins]([TimetableId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sessions_TimetableId'
    AND object_id = OBJECT_ID(N'[dbo].[Sessions]'))
CREATE INDEX [IX_Sessions_TimetableId] ON [dbo].[Sessions]([TimetableId]);
GO
