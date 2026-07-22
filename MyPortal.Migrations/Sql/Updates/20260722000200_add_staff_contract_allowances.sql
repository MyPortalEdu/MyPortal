-- Contract allowances (TLR, SEN, R&R, London weighting) — many per contract, each with its own
-- amount, period and payroll treatment. Previously the AdditionalPaymentTypes lookup had nothing
-- linking it to a contract, so MPS + TLR1 + SEN was unrepresentable. Idempotent.

IF OBJECT_ID(N'[dbo].[StaffContractAllowances]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffContractAllowances] (
    [Id] uniqueidentifier NOT NULL,
    [StaffContractId] uniqueidentifier NOT NULL,
    [AdditionalPaymentTypeId] uniqueidentifier NOT NULL,
    [Amount] decimal(10,2) NOT NULL CONSTRAINT DF_StaffContractAllowances_Amount DEFAULT (0),
    [PayFactor] decimal(5,4) NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [IsSuperannuable] bit NOT NULL CONSTRAINT DF_StaffContractAllowances_IsSuperannuable DEFAULT (0),
    [IsSubjectToNi] bit NOT NULL CONSTRAINT DF_StaffContractAllowances_IsSubjectToNi DEFAULT (0),
    [IsBenefitInKind] bit NOT NULL CONSTRAINT DF_StaffContractAllowances_IsBenefitInKind DEFAULT (0),
    [Reason] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffContractAllowances_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractAllowances_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffContractAllowances_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffContractAllowances_Version DEFAULT (1),
    CONSTRAINT PK_StaffContractAllowances PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractAllowances_StaffContractId_StaffContracts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    ALTER TABLE [dbo].[StaffContractAllowances] ADD CONSTRAINT [FK_StaffContractAllowances_StaffContractId_StaffContracts]
        FOREIGN KEY ([StaffContractId]) REFERENCES [dbo].[StaffContracts]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractAllowances_AdditionalPaymentTypeId_AdditionalPaymentTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    ALTER TABLE [dbo].[StaffContractAllowances] ADD CONSTRAINT [FK_StaffContractAllowances_AdditionalPaymentTypeId_AdditionalPaymentTypes]
        FOREIGN KEY ([AdditionalPaymentTypeId]) REFERENCES [dbo].[AdditionalPaymentTypes]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractAllowances_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    ALTER TABLE [dbo].[StaffContractAllowances] ADD CONSTRAINT [FK_StaffContractAllowances_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContractAllowances_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    ALTER TABLE [dbo].[StaffContractAllowances] ADD CONSTRAINT [FK_StaffContractAllowances_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContractAllowances_StaffContractId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    CREATE INDEX [IX_StaffContractAllowances_StaffContractId] ON [dbo].[StaffContractAllowances]([StaffContractId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContractAllowances_AdditionalPaymentTypeId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContractAllowances]'))
    CREATE INDEX [IX_StaffContractAllowances_AdditionalPaymentTypeId] ON [dbo].[StaffContractAllowances]([AdditionalPaymentTypeId]);
GO
