-- Welfare (safeguarding) core: move the flat looked-after snapshot to dated in-care episodes, and add
-- dated Personal Education Plans (with contributors) and child protection plans. All follow the SIMS
-- welfare shape: a dated child record (StartDate, EndDate?, Comment) with the current state derived from
-- the open (no-end-date) row. Living arrangement is a new lookup, seeded. Care authority / CP authority
-- reuse the existing LocalAuthorities lookup.

IF OBJECT_ID(N'dbo.LivingArrangements', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[LivingArrangements] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL,
    CONSTRAINT PK_LivingArrangements PRIMARY KEY CLUSTERED ([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.StudentCareEpisodes', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentCareEpisodes] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [CaringAuthorityId] uniqueidentifier NOT NULL,
    [LivingArrangementId] uniqueidentifier NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Comment] nvarchar(1024) NULL,
    CONSTRAINT PK_StudentCareEpisodes PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_StudentCareEpisodes_StudentId] ON [dbo].[StudentCareEpisodes]([StudentId]);
END
GO

IF OBJECT_ID(N'dbo.StudentPeps', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentPeps] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Comment] nvarchar(1024) NULL,
    CONSTRAINT PK_StudentPeps PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_StudentPeps_StudentId] ON [dbo].[StudentPeps]([StudentId]);
END
GO

IF OBJECT_ID(N'dbo.StudentPepContributors', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentPepContributors] (
    [Id] uniqueidentifier NOT NULL,
    [StudentPepId] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_StudentPepContributors PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_StudentPepContributors_StudentPepId] ON [dbo].[StudentPepContributors]([StudentPepId]);
END
GO

IF OBJECT_ID(N'dbo.StudentChildProtectionPlans', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentChildProtectionPlans] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [LocalAuthorityId] uniqueidentifier NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Comment] nvarchar(1024) NULL,
    CONSTRAINT PK_StudentChildProtectionPlans PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_StudentChildProtectionPlans_StudentId] ON [dbo].[StudentChildProtectionPlans]([StudentId]);
END
GO

MERGE INTO [dbo].[LivingArrangements] AS Target
    USING (VALUES
        (N'Foster care'),
        (N'Placed with parents'),
        (N'Placed with other relatives or friends'),
        (N'Residential care home'),
        (N'Residential school'),
        (N'Semi-independent living'),
        (N'Secure unit'),
        (N'Other')
    ) AS Source ([Description])
    ON Target.[Description] = Source.[Description]
    WHEN NOT MATCHED BY Target THEN
        INSERT ([Id], [Description], [Active])
        VALUES (NEWID(), Source.[Description], 1);
GO

-- Drop the flat looked-after snapshot now that in-care is dated episodes.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Students_CaringAuthorityId_LocalAuthorities' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [FK_Students_CaringAuthorityId_LocalAuthorities];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_CaringAuthorityId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
    DROP INDEX [IX_Students_CaringAuthorityId] ON [dbo].[Students];
GO

IF COL_LENGTH(N'dbo.Students', N'CaringAuthorityId') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP COLUMN [CaringAuthorityId];
GO

IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Students_InCare' AND parent_object_id = OBJECT_ID(N'[dbo].[Students]'))
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [DF_Students_InCare];
GO

IF COL_LENGTH(N'dbo.Students', N'InCare') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP COLUMN [InCare];
GO
