-- ============================================================================
-- Scheduled annual increments.
--
-- A scheduled run is (service term, effective date). It is NOT a frozen list of
-- moves: eligibility is re-computed when it is applied, so leavers, promotions
-- and manual point changes between scheduling and the date are reflected.
--
-- Also stamps StaffContractSalaryChanges with the increment's effective date and
-- a source marker, so a given increment cycle can't be applied to the same
-- contract twice (whether from a re-run or a scheduled + manual overlap).
-- ============================================================================

IF OBJECT_ID(N'[dbo].[ScheduledIncrements]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ScheduledIncrements] (
    [Id] uniqueidentifier NOT NULL,
    [ServiceTermId] uniqueidentifier NOT NULL,
    [EffectiveDate] date NOT NULL,
    -- Scheduled | Completed | Cancelled
    [Status] nvarchar(20) NOT NULL CONSTRAINT DF_ScheduledIncrements_Status DEFAULT ('Scheduled'),
    [CompletedAt] datetime2(7) NULL,
    [AppliedCount] int NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_ScheduledIncrements_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_ScheduledIncrements_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ScheduledIncrements PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ScheduledIncrements_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[ScheduledIncrements]'))
    ALTER TABLE [dbo].[ScheduledIncrements] ADD CONSTRAINT [FK_ScheduledIncrements_ServiceTermId_ServiceTerms]
        FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScheduledIncrements_Status_EffectiveDate'
    AND object_id = OBJECT_ID(N'[dbo].[ScheduledIncrements]'))
    CREATE INDEX [IX_ScheduledIncrements_Status_EffectiveDate]
        ON [dbo].[ScheduledIncrements]([Status], [EffectiveDate]);
GO

-- ---- StaffContractSalaryChanges idempotency stamps ----
IF COL_LENGTH(N'[dbo].[StaffContractSalaryChanges]', N'EffectiveDate') IS NULL
    ALTER TABLE [dbo].[StaffContractSalaryChanges] ADD [EffectiveDate] date NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffContractSalaryChanges]', N'Source') IS NULL
    ALTER TABLE [dbo].[StaffContractSalaryChanges] ADD [Source] nvarchar(20) NULL;
GO

-- Finds a contract already incremented for a given effective date fast.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContractSalaryChanges_Increment'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContractSalaryChanges]'))
    CREATE INDEX [IX_StaffContractSalaryChanges_Increment]
        ON [dbo].[StaffContractSalaryChanges]([StaffContractId], [EffectiveDate])
        WHERE [Source] = 'Increment';
GO
