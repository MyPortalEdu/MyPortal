-- Training events + a nullable TrainingEventId on TrainingCertificates (booking = certificate). Idempotent.

IF OBJECT_ID(N'[dbo].[TrainingEvents]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TrainingEvents] (
    [Id] uniqueidentifier NOT NULL,
    [TrainingCourseId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Venue] nvarchar(200) NULL,
    [Trainer] nvarchar(200) NULL,
    [Provider] nvarchar(200) NULL,
    [Hours] decimal(9, 2) NULL,
    [Capacity] int NULL,
    [Notes] nvarchar(1000) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_TrainingEvents_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_TrainingEvents_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_TrainingEvents_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_TrainingEvents_Version DEFAULT (1),
    CONSTRAINT PK_TrainingEvents PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingEvents_TrainingCourseId_TrainingCourses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingEvents]'))
    ALTER TABLE [dbo].[TrainingEvents] ADD CONSTRAINT [FK_TrainingEvents_TrainingCourseId_TrainingCourses]
        FOREIGN KEY ([TrainingCourseId]) REFERENCES [dbo].[TrainingCourses]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingEvents_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingEvents]'))
    ALTER TABLE [dbo].[TrainingEvents] ADD CONSTRAINT [FK_TrainingEvents_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingEvents_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingEvents]'))
    ALTER TABLE [dbo].[TrainingEvents] ADD CONSTRAINT [FK_TrainingEvents_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingEvents_TrainingCourseId'
    AND object_id = OBJECT_ID(N'[dbo].[TrainingEvents]'))
    CREATE INDEX [IX_TrainingEvents_TrainingCourseId] ON [dbo].[TrainingEvents]([TrainingCourseId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingEvents_StartDate'
    AND object_id = OBJECT_ID(N'[dbo].[TrainingEvents]'))
    CREATE INDEX [IX_TrainingEvents_StartDate] ON [dbo].[TrainingEvents]([StartDate]);
GO

-- ---- Attendees hang off TrainingCertificates via a new event link ----
IF COL_LENGTH(N'[dbo].[TrainingCertificates]', N'TrainingEventId') IS NULL
    ALTER TABLE [dbo].[TrainingCertificates] ADD [TrainingEventId] uniqueidentifier NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingCertificates_TrainingEventId_TrainingEvents'
    AND parent_object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
    ALTER TABLE [dbo].[TrainingCertificates] ADD CONSTRAINT [FK_TrainingCertificates_TrainingEventId_TrainingEvents]
        FOREIGN KEY ([TrainingEventId]) REFERENCES [dbo].[TrainingEvents]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainingCertificates_TrainingEventId'
    AND object_id = OBJECT_ID(N'[dbo].[TrainingCertificates]'))
    CREATE INDEX [IX_TrainingCertificates_TrainingEventId] ON [dbo].[TrainingCertificates]([TrainingEventId])
        WHERE [TrainingEventId] IS NOT NULL;
GO
