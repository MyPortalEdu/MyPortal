-- Service terms become real reference data rather than a name-only lookup: pay-spine settings,
-- default hours/weeks, increment timing and the pension schemes each term offers.
--
-- Deliberately NOT carried over from SIMS: the monthly pay pattern (Jan..Dec weeks worked +
-- monthly reconciliation) exists only to spread commitment into their FMS finance module, and
-- award-by-salary-range belongs with the scales-to-ranges conversion. Idempotent.

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'Code') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [Code] nvarchar(16) NULL;
GO

-- Backfill the starter terms with their recognised framework codes before Code goes NOT NULL.
UPDATE [dbo].[ServiceTerms] SET [Code] = N'BURGUNDY'
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000001' AND [Code] IS NULL;
UPDATE [dbo].[ServiceTerms] SET [Code] = N'NJC'
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000002' AND [Code] IS NULL;
UPDATE [dbo].[ServiceTerms] SET [Code] = N'SOULBURY'
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000003' AND [Code] IS NULL;
UPDATE [dbo].[ServiceTerms] SET [Code] = N'SFC'
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000004' AND [Code] IS NULL;
UPDATE [dbo].[ServiceTerms] SET [Code] = N'INDEP'
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000005' AND [Code] IS NULL;
GO

-- Anything else (a term added before this migration) gets a unique fallback rather than a guess.
UPDATE [dbo].[ServiceTerms]
    SET [Code] = N'ST' + UPPER(RIGHT(CONVERT(nvarchar(36), [Id]), 6))
    WHERE [Code] IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ServiceTerms]')
    AND name = N'Code' AND is_nullable = 1)
    ALTER TABLE [dbo].[ServiceTerms] ALTER COLUMN [Code] nvarchar(16) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ServiceTerms_Code'
    AND object_id = OBJECT_ID(N'[dbo].[ServiceTerms]'))
    CREATE UNIQUE INDEX [UX_ServiceTerms_Code] ON [dbo].[ServiceTerms]([Code]);
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'IsTeacher') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [IsTeacher] bit NOT NULL
        CONSTRAINT DF_ServiceTerms_IsTeacher DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'Salaried') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [Salaried] bit NOT NULL
        CONSTRAINT DF_ServiceTerms_Salaried DEFAULT (1);
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'SpinalProgression') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [SpinalProgression] bit NOT NULL
        CONSTRAINT DF_ServiceTerms_SpinalProgression DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'SinglePaySpine') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [SinglePaySpine] bit NOT NULL
        CONSTRAINT DF_ServiceTerms_SinglePaySpine DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'TermTimeOnlyPossible') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [TermTimeOnlyPossible] bit NOT NULL
        CONSTRAINT DF_ServiceTerms_TermTimeOnlyPossible DEFAULT (0);
GO

-- Increment timing: month + day the spine point advances. Null when SpinalProgression is off.
IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'IncrementMonth') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [IncrementMonth] tinyint NULL;
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'IncrementDay') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [IncrementDay] tinyint NULL;
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'MinimumPoint') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [MinimumPoint] decimal(6,2) NULL;
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'MaximumPoint') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [MaximumPoint] decimal(6,2) NULL;
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'PointInterval') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [PointInterval] decimal(6,2) NULL;
GO

-- Defaults pre-filled onto a new contract for this term; overtypeable per contract.
IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'HoursPerWeek') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [HoursPerWeek] decimal(5,2) NULL;
GO

IF COL_LENGTH(N'[dbo].[ServiceTerms]', N'WeeksPerYear') IS NULL
    ALTER TABLE [dbo].[ServiceTerms] ADD [WeeksPerYear] decimal(4,2) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ServiceTerms_IncrementMonth')
    ALTER TABLE [dbo].[ServiceTerms] ADD CONSTRAINT [CK_ServiceTerms_IncrementMonth]
        CHECK ([IncrementMonth] IS NULL OR [IncrementMonth] BETWEEN 1 AND 12);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ServiceTerms_IncrementDay')
    ALTER TABLE [dbo].[ServiceTerms] ADD CONSTRAINT [CK_ServiceTerms_IncrementDay]
        CHECK ([IncrementDay] IS NULL OR [IncrementDay] BETWEEN 1 AND 31);
GO

-- ---- Pension schemes offered by a service term ----
IF OBJECT_ID(N'[dbo].[ServiceTermSuperannuationSchemes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ServiceTermSuperannuationSchemes] (
    [Id] uniqueidentifier NOT NULL,
    [ServiceTermId] uniqueidentifier NOT NULL,
    [SuperannuationSchemeId] uniqueidentifier NOT NULL,
    -- The scheme a new contract on this term defaults to; at most one per term.
    [IsMain] bit NOT NULL CONSTRAINT DF_ServiceTermSuperannuationSchemes_IsMain DEFAULT (0),
    CONSTRAINT PK_ServiceTermSuperannuationSchemes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ServiceTermSuperannuationSchemes_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[ServiceTermSuperannuationSchemes]'))
    ALTER TABLE [dbo].[ServiceTermSuperannuationSchemes] ADD CONSTRAINT [FK_ServiceTermSuperannuationSchemes_ServiceTermId_ServiceTerms]
        FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ServiceTermSuperannuationSchemes_SuperannuationSchemeId_SuperannuationSchemes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[ServiceTermSuperannuationSchemes]'))
    ALTER TABLE [dbo].[ServiceTermSuperannuationSchemes] ADD CONSTRAINT [FK_ServiceTermSuperannuationSchemes_SuperannuationSchemeId_SuperannuationSchemes]
        FOREIGN KEY ([SuperannuationSchemeId]) REFERENCES [dbo].[SuperannuationSchemes]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ServiceTermSuperannuationSchemes_TermScheme'
    AND object_id = OBJECT_ID(N'[dbo].[ServiceTermSuperannuationSchemes]'))
    CREATE UNIQUE INDEX [UX_ServiceTermSuperannuationSchemes_TermScheme]
        ON [dbo].[ServiceTermSuperannuationSchemes]([ServiceTermId], [SuperannuationSchemeId]);
GO

-- At most one main scheme per term, enforced in the database rather than only in the service.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ServiceTermSuperannuationSchemes_OneMain'
    AND object_id = OBJECT_ID(N'[dbo].[ServiceTermSuperannuationSchemes]'))
    CREATE UNIQUE INDEX [UX_ServiceTermSuperannuationSchemes_OneMain]
        ON [dbo].[ServiceTermSuperannuationSchemes]([ServiceTermId]) WHERE [IsMain] = 1;
GO

-- ---- Classify the starter terms so the defaults are useful out of the box ----
UPDATE [dbo].[ServiceTerms]
    SET [IsTeacher] = 1, [SpinalProgression] = 1, [IncrementMonth] = 9, [IncrementDay] = 1,
        [WeeksPerYear] = 52, [Salaried] = 1
    WHERE [Id] IN (N'B2A3C4D5-0001-4000-8000-000000000001',
                   N'B2A3C4D5-0001-4000-8000-000000000004')
      AND [IsTeacher] = 0;
GO

UPDATE [dbo].[ServiceTerms]
    SET [SpinalProgression] = 1, [IncrementMonth] = 4, [IncrementDay] = 1,
        [HoursPerWeek] = 37, [WeeksPerYear] = 52, [TermTimeOnlyPossible] = 1
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000002'
      AND [SpinalProgression] = 0;
GO

UPDATE [dbo].[ServiceTerms]
    SET [SpinalProgression] = 1, [IncrementMonth] = 4, [IncrementDay] = 1, [WeeksPerYear] = 52
    WHERE [Id] = N'B2A3C4D5-0001-4000-8000-000000000003'
      AND [SpinalProgression] = 0;
GO
