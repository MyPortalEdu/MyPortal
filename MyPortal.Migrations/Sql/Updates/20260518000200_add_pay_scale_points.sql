-- ============================================================================
-- Pay scale points + zone-aware statutory rates.
--
-- Adds the structured side of the pay-scale model:
--
--   * PayZones — geographic banding (Inner London / Outer London / Fringe /
--     Rest of England). The zone a school sits in is a function of its
--     postcode, so it lives on School, not on Contract.
--
--   * PayScalePoints — one row per discrete point (M1..M6, U1..U3, L1..L43,
--     UNQ1..UNQ6). Hangs off PayScale.
--
--   * PayScalePointRates — effective-dated salary for a (point, zone) pair.
--     Annual pay awards close the current row by setting EffectiveTo and
--     insert a new row with the new EffectiveFrom; rates are never mutated
--     in place so history stays intact for back-dated audits.
--
-- Also adds:
--   * Schools.PayZoneId so the school's location drives rate selection.
--   * StaffContracts.PayScalePointId for structured pay point selection,
--     alongside the existing free-text SpinePoint which remains as a
--     fallback for locally determined / non-statutory pay.
-- ============================================================================

-- ============================================================================
-- Tables
-- ============================================================================

IF OBJECT_ID(N'[dbo].[PayZones]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PayZones] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_PayZones_Active DEFAULT (1),
    [Code] nvarchar(10) NOT NULL,
    CONSTRAINT PK_PayZones PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[PayScalePoints]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PayScalePoints] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_PayScalePoints_Active DEFAULT (1),
    [PayScaleId] uniqueidentifier NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_PayScalePoints_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_PayScalePoints PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[PayScalePointRates]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PayScalePointRates] (
    [Id] uniqueidentifier NOT NULL,
    [PayScalePointId] uniqueidentifier NOT NULL,
    [PayZoneId] uniqueidentifier NOT NULL,
    [EffectiveFrom] date NOT NULL,
    [EffectiveTo] date NULL,
    [AnnualSalary] decimal(10,2) NOT NULL,
    CONSTRAINT PK_PayScalePointRates PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- Schools.PayZoneId
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Schools') AND name = N'PayZoneId'
)
BEGIN
    ALTER TABLE dbo.Schools ADD PayZoneId uniqueidentifier NULL;
END
GO

-- ============================================================================
-- StaffContracts.PayScalePointId
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffContracts') AND name = N'PayScalePointId'
)
BEGIN
    ALTER TABLE dbo.StaffContracts ADD PayScalePointId uniqueidentifier NULL;
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PayScalePoints_PayScaleId_PayScales'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
ALTER TABLE [dbo].[PayScalePoints]
    ADD CONSTRAINT [FK_PayScalePoints_PayScaleId_PayScales]
    FOREIGN KEY ([PayScaleId]) REFERENCES [dbo].[PayScales]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PayScalePointRates_PayScalePointId_PayScalePoints'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PayScalePointRates]'))
BEGIN
ALTER TABLE [dbo].[PayScalePointRates]
    ADD CONSTRAINT [FK_PayScalePointRates_PayScalePointId_PayScalePoints]
    FOREIGN KEY ([PayScalePointId]) REFERENCES [dbo].[PayScalePoints]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PayScalePointRates_PayZoneId_PayZones'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PayScalePointRates]'))
BEGIN
ALTER TABLE [dbo].[PayScalePointRates]
    ADD CONSTRAINT [FK_PayScalePointRates_PayZoneId_PayZones]
    FOREIGN KEY ([PayZoneId]) REFERENCES [dbo].[PayZones]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Schools_PayZoneId_PayZones'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Schools]'))
BEGIN
ALTER TABLE [dbo].[Schools]
    ADD CONSTRAINT [FK_Schools_PayZoneId_PayZones]
    FOREIGN KEY ([PayZoneId]) REFERENCES [dbo].[PayZones]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_PayScalePointId_PayScalePoints'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_PayScalePointId_PayScalePoints]
    FOREIGN KEY ([PayScalePointId]) REFERENCES [dbo].[PayScalePoints]([Id]);
END
GO

-- ============================================================================
-- Indexes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayScalePoints_PayScaleId'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
CREATE INDEX [IX_PayScalePoints_PayScaleId]
    ON [dbo].[PayScalePoints]([PayScaleId]);
END
GO

-- Covering index for the common lookup: "what's the current rate for this
-- point in this zone?" Filtered to open-ended rows so it stays small.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayScalePointRates_PointZone_Current'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePointRates]'))
BEGIN
CREATE INDEX [IX_PayScalePointRates_PointZone_Current]
    ON [dbo].[PayScalePointRates]([PayScalePointId], [PayZoneId], [EffectiveFrom])
    INCLUDE ([AnnualSalary])
    WHERE [EffectiveTo] IS NULL;
END
GO
