-- Statutory / payroll side of staff absence: certificates and fit notes, authorised pay rate,
-- payroll category, SSP exclusion, and days/hours lost. The pay treatment is gated to All-scope
-- (HR) editors at the service layer. Idempotent.

-- ---- Lookups ----
IF OBJECT_ID(N'[dbo].[StaffAbsencePayRates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffAbsencePayRates] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_StaffAbsencePayRates_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_StaffAbsencePayRates_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_StaffAbsencePayRates_Active DEFAULT (1),
    CONSTRAINT PK_StaffAbsencePayRates PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffAbsencePayrollReasons]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffAbsencePayrollReasons] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_StaffAbsencePayrollReasons_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_StaffAbsencePayrollReasons_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_StaffAbsencePayrollReasons_Active DEFAULT (1),
    CONSTRAINT PK_StaffAbsencePayrollReasons PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ---- Certificates ----
IF OBJECT_ID(N'[dbo].[StaffAbsenceCertificates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffAbsenceCertificates] (
    [Id] uniqueidentifier NOT NULL,
    [StaffAbsenceId] uniqueidentifier NOT NULL,
    [DateReceived] datetime2(7) NOT NULL,
    [DateSigned] datetime2(7) NULL,
    [IsSelfCertified] bit NOT NULL CONSTRAINT DF_StaffAbsenceCertificates_IsSelfCertified DEFAULT (0),
    [IsReturnToWork] bit NOT NULL CONSTRAINT DF_StaffAbsenceCertificates_IsReturnToWork DEFAULT (0),
    [SignedBy] nvarchar(256) NULL,
    [Notes] nvarchar(256) NULL,
    CONSTRAINT PK_StaffAbsenceCertificates PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsenceCertificates_StaffAbsenceId_StaffAbsences'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsenceCertificates]'))
    ALTER TABLE [dbo].[StaffAbsenceCertificates] ADD CONSTRAINT [FK_StaffAbsenceCertificates_StaffAbsenceId_StaffAbsences]
        FOREIGN KEY ([StaffAbsenceId]) REFERENCES [dbo].[StaffAbsences]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffAbsenceCertificates_StaffAbsenceId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffAbsenceCertificates]'))
    CREATE INDEX [IX_StaffAbsenceCertificates_StaffAbsenceId] ON [dbo].[StaffAbsenceCertificates]([StaffAbsenceId]);
GO

-- ---- New StaffAbsences columns ----
IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'AuthorisedPayRateId') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [AuthorisedPayRateId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'PayrollReasonId') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [PayrollReasonId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'SspExcluded') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [SspExcluded] bit NOT NULL CONSTRAINT DF_StaffAbsences_SspExcluded DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'WorkingDaysLost') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [WorkingDaysLost] decimal(6,2) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'HoursLost') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [HoursLost] decimal(6,2) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffAbsences]', N'IsIndustrialInjury') IS NULL
    ALTER TABLE [dbo].[StaffAbsences] ADD [IsIndustrialInjury] bit NOT NULL CONSTRAINT DF_StaffAbsences_IsIndustrialInjury DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsences_AuthorisedPayRateId_StaffAbsencePayRates'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
    ALTER TABLE [dbo].[StaffAbsences] ADD CONSTRAINT [FK_StaffAbsences_AuthorisedPayRateId_StaffAbsencePayRates]
        FOREIGN KEY ([AuthorisedPayRateId]) REFERENCES [dbo].[StaffAbsencePayRates]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffAbsences_PayrollReasonId_StaffAbsencePayrollReasons'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffAbsences]'))
    ALTER TABLE [dbo].[StaffAbsences] ADD CONSTRAINT [FK_StaffAbsences_PayrollReasonId_StaffAbsencePayrollReasons]
        FOREIGN KEY ([PayrollReasonId]) REFERENCES [dbo].[StaffAbsencePayrollReasons]([Id]);
GO

-- ---- Seeds (system rows, protected from edit/delete) ----
MERGE INTO dbo.StaffAbsencePayRates AS Target
USING (VALUES
    (N'5A11FA7E-0000-4000-8000-000000000010', N'Full pay',                        10),
    (N'5A11FA7E-0000-4000-8000-000000000020', N'Occupational sick pay — full pay', 20),
    (N'5A11FA7E-0000-4000-8000-000000000030', N'Occupational sick pay — half pay', 30),
    (N'5A11FA7E-0000-4000-8000-000000000040', N'Statutory Sick Pay (SSP) only',   40),
    (N'5A11FA7E-0000-4000-8000-000000000050', N'Statutory Maternity/Paternity Pay', 50),
    (N'5A11FA7E-0000-4000-8000-000000000060', N'Unpaid',                          60),
    (N'5A11FA7E-0000-4000-8000-000000000070', N'Not yet assessed',               800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO

MERGE INTO dbo.StaffAbsencePayrollReasons AS Target
USING (VALUES
    (N'5A11B011-0000-4000-8000-000000000010', N'Sickness',                    10),
    (N'5A11B011-0000-4000-8000-000000000020', N'Industrial injury',           20),
    (N'5A11B011-0000-4000-8000-000000000030', N'Maternity / paternity leave', 30),
    (N'5A11B011-0000-4000-8000-000000000040', N'Adoption leave',              40),
    (N'5A11B011-0000-4000-8000-000000000050', N'Parental / dependants leave',  50),
    (N'5A11B011-0000-4000-8000-000000000060', N'Compassionate leave',         60),
    (N'5A11B011-0000-4000-8000-000000000070', N'Public duties',               70),
    (N'5A11B011-0000-4000-8000-000000000080', N'Jury service',                80),
    (N'5A11B011-0000-4000-8000-000000000090', N'Unpaid leave',                90),
    (N'5A11B011-0000-4000-8000-0000000000A0', N'Other',                      800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO
