-- SEN statutory plans (EHC plans / legacy statements): the assessment lifecycle record with its
-- legal/dispute detail, plus the two statutory-assessment lookups (agreed + outcome), seeded.

IF OBJECT_ID(N'dbo.SenStatutoryAssessmentAgreed', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenStatutoryAssessmentAgreed] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL,
    CONSTRAINT PK_SenStatutoryAssessmentAgreed PRIMARY KEY CLUSTERED ([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.SenStatutoryAssessmentOutcome', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenStatutoryAssessmentOutcome] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL,
    CONSTRAINT PK_SenStatutoryAssessmentOutcome PRIMARY KEY CLUSTERED ([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.SenStatements', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SenStatements] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [IsEhcp] bit NOT NULL,
    [AssessmentRequestDate] datetime2(7) NOT NULL,
    [ParentConsultDate] datetime2(7) NULL,
    [FinalisedDate] datetime2(7) NULL,
    [CeasedDate] datetime2(7) NULL,
    [StatutoryAssessmentAgreedId] uniqueidentifier NULL,
    [StatutoryAssessmentOutcomeId] uniqueidentifier NULL,
    [SubjectToTribunal] bit NOT NULL,
    [UndergoingMediation] bit NOT NULL,
    [AppealNotes] nvarchar(1024) NULL,
    [TemporaryDisapplicationSubjects] nvarchar(256) NULL,
    [PermanentDisapplicationSubjects] nvarchar(256) NULL,
    [LocalAuthorityId] uniqueidentifier NULL,
    [Comments] nvarchar(1024) NULL,
    CONSTRAINT PK_SenStatements PRIMARY KEY CLUSTERED ([Id])
    );
CREATE INDEX [IX_SenStatements_StudentId] ON [dbo].[SenStatements]([StudentId]);
END
GO

MERGE INTO [dbo].[SenStatutoryAssessmentAgreed] AS Target
    USING (VALUES
        (N'Assessment agreed'),
        (N'Assessment not agreed'),
        (N'Awaiting decision')
    ) AS Source ([Description])
    ON Target.[Description] = Source.[Description]
    WHEN NOT MATCHED BY Target THEN
        INSERT ([Id], [Description], [Active])
        VALUES (NEWID(), Source.[Description], 1);
GO

MERGE INTO [dbo].[SenStatutoryAssessmentOutcome] AS Target
    USING (VALUES
        (N'EHC plan issued'),
        (N'No plan issued'),
        (N'Note in lieu of a plan'),
        (N'Plan ceased'),
        (N'Assessment discontinued')
    ) AS Source ([Description])
    ON Target.[Description] = Source.[Description]
    WHEN NOT MATCHED BY Target THEN
        INSERT ([Id], [Description], [Active])
        VALUES (NEWID(), Source.[Description], 1);
GO
