-- SEN core: move the flat single SEN type on the pupil to dated, ranked need records; add a dated SEN
-- status history (the pupil's SenStatusId becomes a denormalised cache of the current row); add review
-- participants; and enrich provisions with frequency/cost and an open-ended (nullable) end date.

-- Dated, ranked SEN needs (replaces the single Students.SenTypeId scalar).
IF OBJECT_ID(N'dbo.StudentSenNeeds', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentSenNeeds] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SenTypeId] uniqueidentifier NOT NULL,
    [Description] nvarchar(1024) NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Rank] int NOT NULL,
    CONSTRAINT PK_StudentSenNeeds PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_StudentSenNeeds_StudentId] ON [dbo].[StudentSenNeeds]([StudentId]);
END
GO

-- Dated SEN status/stage history; Students.SenStatusId caches the current (open) row.
IF OBJECT_ID(N'dbo.SenStatusHistories', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenStatusHistories] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [SenStatusId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    CONSTRAINT PK_SenStatusHistories PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_SenStatusHistories_StudentId] ON [dbo].[SenStatusHistories]([StudentId]);
END
GO

-- People invited to / attending a SEN review.
IF OBJECT_ID(N'dbo.SenReviewParticipants', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenReviewParticipants] (
    [Id] uniqueidentifier NOT NULL,
    [SenReviewId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [Invited] bit NOT NULL,
    [Attended] bit NOT NULL,
    CONSTRAINT PK_SenReviewParticipants PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_SenReviewParticipants_SenReviewId] ON [dbo].[SenReviewParticipants]([SenReviewId]);
END
GO

-- Enrich provisions: delivery frequency, cost, and open-ended (nullable) end date.
IF COL_LENGTH(N'dbo.SenProvisions', N'Frequency') IS NULL
    ALTER TABLE [dbo].[SenProvisions] ADD [Frequency] nvarchar(128) NULL;
GO

IF COL_LENGTH(N'dbo.SenProvisions', N'Cost') IS NULL
    ALTER TABLE [dbo].[SenProvisions] ADD [Cost] decimal(18, 2) NULL;
GO

ALTER TABLE [dbo].[SenProvisions] ALTER COLUMN [EndDate] datetime2(7) NULL;
GO

-- Drop the flat single SEN type on the pupil, now that needs are dated/ranked records.
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_SenTypeId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
    DROP INDEX [IX_Students_SenTypeId] ON [dbo].[Students];
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_SenTypeId_SenTypes' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [FK_Students_SenTypeId_SenTypes];
GO

IF COL_LENGTH(N'dbo.Students', N'SenTypeId') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP COLUMN [SenTypeId];
GO
