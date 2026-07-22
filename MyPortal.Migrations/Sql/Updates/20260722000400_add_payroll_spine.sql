-- ============================================================================
-- Payroll spine completion — the three remaining High-priority contract gaps:
--   * SuperannuationSchemes / SuperannuationSchemeRates — pension scheme per
--     contract + effective-dated employer contribution rate (same close-and-insert
--     shape as PayScalePointRates). Plus StaffContracts.SuperannuationSchemeId and
--     NiContractedOut.
--   * StaffContractSuspensions — date-ranged suspension periods (disciplinary /
--     medical), many per contract.
--   * StaffContractSalaryChanges — append-only log of pay-point / salary movement,
--     written by the service; the audit stamp is the changed-by / changed-on.
--
-- No new permissions — all inside the existing EmploymentDetails area. Idempotent.
-- ============================================================================

IF OBJECT_ID(N'[dbo].[SuperannuationSchemes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SuperannuationSchemes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_SuperannuationSchemes_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_SuperannuationSchemes_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_SuperannuationSchemes_Active DEFAULT (1),
    CONSTRAINT PK_SuperannuationSchemes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[SuperannuationSchemeRates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SuperannuationSchemeRates] (
    [Id] uniqueidentifier NOT NULL,
    [SuperannuationSchemeId] uniqueidentifier NOT NULL,
    [EffectiveFrom] datetime2(7) NOT NULL,
    [EffectiveTo] datetime2(7) NULL,
    [EmployerRate] decimal(5,2) NOT NULL,
    CONSTRAINT PK_SuperannuationSchemeRates PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SuperannuationSchemeRates_SuperannuationSchemeId_SuperannuationSchemes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[SuperannuationSchemeRates]'))
    ALTER TABLE [dbo].[SuperannuationSchemeRates] ADD CONSTRAINT [FK_SuperannuationSchemeRates_SuperannuationSchemeId_SuperannuationSchemes]
        FOREIGN KEY ([SuperannuationSchemeId]) REFERENCES [dbo].[SuperannuationSchemes]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SuperannuationSchemeRates_SuperannuationSchemeId'
    AND object_id = OBJECT_ID(N'[dbo].[SuperannuationSchemeRates]'))
    CREATE INDEX [IX_SuperannuationSchemeRates_SuperannuationSchemeId] ON [dbo].[SuperannuationSchemeRates]([SuperannuationSchemeId]);
GO

-- ---- Suspensions ----
IF OBJECT_ID(N'[dbo].[StaffContractSuspensions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffContractSuspensions] (
    [Id] uniqueidentifier NOT NULL,
    [StaffContractId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Reason] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffContractSuspensions_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractSuspensions_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractSuspensions_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffContractSuspensions_Version DEFAULT (1),
    CONSTRAINT PK_StaffContractSuspensions PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractSuspensions_StaffContractId_StaffContracts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractSuspensions]'))
    ALTER TABLE [dbo].[StaffContractSuspensions] ADD CONSTRAINT [FK_StaffContractSuspensions_StaffContractId_StaffContracts]
        FOREIGN KEY ([StaffContractId]) REFERENCES [dbo].[StaffContracts]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractSuspensions_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractSuspensions]'))
    ALTER TABLE [dbo].[StaffContractSuspensions] ADD CONSTRAINT [FK_StaffContractSuspensions_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractSuspensions_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractSuspensions]'))
    ALTER TABLE [dbo].[StaffContractSuspensions] ADD CONSTRAINT [FK_StaffContractSuspensions_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContractSuspensions_StaffContractId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContractSuspensions]'))
    CREATE INDEX [IX_StaffContractSuspensions_StaffContractId] ON [dbo].[StaffContractSuspensions]([StaffContractId]);
GO

-- ---- Salary change log ----
IF OBJECT_ID(N'[dbo].[StaffContractSalaryChanges]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffContractSalaryChanges] (
    [Id] uniqueidentifier NOT NULL,
    [StaffContractId] uniqueidentifier NOT NULL,
    [OldPayScalePointId] uniqueidentifier NULL,
    [NewPayScalePointId] uniqueidentifier NULL,
    [OldAnnualSalary] decimal(10,2) NULL,
    [NewAnnualSalary] decimal(10,2) NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractSalaryChanges_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractSalaryChanges_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_StaffContractSalaryChanges PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractSalaryChanges_StaffContractId_StaffContracts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractSalaryChanges]'))
    ALTER TABLE [dbo].[StaffContractSalaryChanges] ADD CONSTRAINT [FK_StaffContractSalaryChanges_StaffContractId_StaffContracts]
        FOREIGN KEY ([StaffContractId]) REFERENCES [dbo].[StaffContracts]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContractSalaryChanges_StaffContractId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContractSalaryChanges]'))
    CREATE INDEX [IX_StaffContractSalaryChanges_StaffContractId] ON [dbo].[StaffContractSalaryChanges]([StaffContractId]);
GO

-- ---- New StaffContracts columns ----
IF COL_LENGTH(N'[dbo].[StaffContracts]', N'SuperannuationSchemeId') IS NULL
    ALTER TABLE [dbo].[StaffContracts] ADD [SuperannuationSchemeId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffContracts]', N'NiContractedOut') IS NULL
    ALTER TABLE [dbo].[StaffContracts] ADD [NiContractedOut] bit NOT NULL CONSTRAINT DF_StaffContracts_NiContractedOut DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_SuperannuationSchemeId_SuperannuationSchemes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
    ALTER TABLE [dbo].[StaffContracts] ADD CONSTRAINT [FK_StaffContracts_SuperannuationSchemeId_SuperannuationSchemes]
        FOREIGN KEY ([SuperannuationSchemeId]) REFERENCES [dbo].[SuperannuationSchemes]([Id]);
GO

-- ---- Seeds ----
MERGE INTO dbo.SuperannuationSchemes AS Target
USING (VALUES
    (N'5CBE3E00-0000-4000-8000-000000000010', N'Teachers'' Pension Scheme',           N'TPS',  10),
    (N'5CBE3E00-0000-4000-8000-000000000020', N'Local Government Pension Scheme',     N'LGPS', 20),
    (N'5CBE3E00-0000-4000-8000-000000000030', N'NHS Pension Scheme',                  N'NHS',  30),
    (N'5CBE3E00-0000-4000-8000-000000000040', N'Workplace pension (auto-enrolment)',  N'AE',   40),
    (N'5CBE3E00-0000-4000-8000-000000000050', N'Opted out',                           N'OUT', 800),
    (N'5CBE3E00-0000-4000-8000-000000000060', N'Not eligible',                        N'NEL', 810)
) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Code = Source.Code,
               DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, Code, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.Code, Source.DisplayOrder, 1, 1);
GO

-- Employer contribution rates currently in force (England). Effective-dated: a future rate change
-- closes these rows (set EffectiveTo) and inserts new ones rather than editing in place.
MERGE INTO dbo.SuperannuationSchemeRates AS Target
USING (VALUES
    (N'5CBE3A7E-0000-4000-8000-000000000010', N'5CBE3E00-0000-4000-8000-000000000010', '2024-04-01', 28.68),
    (N'5CBE3A7E-0000-4000-8000-000000000020', N'5CBE3E00-0000-4000-8000-000000000020', '2024-04-01', 20.00),
    (N'5CBE3A7E-0000-4000-8000-000000000030', N'5CBE3E00-0000-4000-8000-000000000040', '2024-04-01',  3.00)
) AS Source (Id, SuperannuationSchemeId, EffectiveFrom, EmployerRate)
    ON Target.Id = Source.Id
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, SuperannuationSchemeId, EffectiveFrom, EffectiveTo, EmployerRate)
    VALUES (Source.Id, Source.SuperannuationSchemeId, Source.EffectiveFrom, NULL, Source.EmployerRate);
GO
