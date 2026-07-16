-- ============================================================================
-- New CBDS-backed lookups that complement existing entities:
--   StaffRoles            (CS050) -> StaffContracts.StaffRoleId
--   QtsRoutes             (CS069) -> StaffMembers.QtsRouteId
--   AdditionalPaymentTypes(CS082) -> (catalogue; consumed by a later pay feature)
--   ResultTypes           (CS014) -> Results.ResultTypeId
-- Each carries Code (DfE) + DisplayOrder (alphabetical default). Seeded in
-- 20260626000410. All FK columns are nullable and additive. Idempotent.
-- ============================================================================

-- StaffRoles -----------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[StaffRoles]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_StaffRoles_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_StaffRoles_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_StaffRoles PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- QtsRoutes ------------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[QtsRoutes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[QtsRoutes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_QtsRoutes_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_QtsRoutes_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_QtsRoutes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- AdditionalPaymentTypes -----------------------------------------------------
IF OBJECT_ID(N'[dbo].[AdditionalPaymentTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[AdditionalPaymentTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_AdditionalPaymentTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_AdditionalPaymentTypes_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_AdditionalPaymentTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ResultTypes ----------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[ResultTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ResultTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_ResultTypes_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ResultTypes_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_ResultTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- FK columns (nullable, additive) --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffContracts') AND name = N'StaffRoleId')
BEGIN
    ALTER TABLE dbo.StaffContracts ADD StaffRoleId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StaffMembers') AND name = N'QtsRouteId')
BEGIN
    ALTER TABLE dbo.StaffMembers ADD QtsRouteId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Results') AND name = N'ResultTypeId')
BEGIN
    ALTER TABLE dbo.Results ADD ResultTypeId uniqueidentifier NULL;
END
GO

-- Foreign keys ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_StaffRoleId_StaffRoles'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
BEGIN
ALTER TABLE [dbo].[StaffContracts]
    ADD CONSTRAINT [FK_StaffContracts_StaffRoleId_StaffRoles]
    FOREIGN KEY ([StaffRoleId]) REFERENCES [dbo].[StaffRoles]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_QtsRouteId_QtsRoutes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
BEGIN
ALTER TABLE [dbo].[StaffMembers]
    ADD CONSTRAINT [FK_StaffMembers_QtsRouteId_QtsRoutes]
    FOREIGN KEY ([QtsRouteId]) REFERENCES [dbo].[QtsRoutes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Results_ResultTypeId_ResultTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Results]'))
BEGIN
ALTER TABLE [dbo].[Results]
    ADD CONSTRAINT [FK_Results_ResultTypeId_ResultTypes]
    FOREIGN KEY ([ResultTypeId]) REFERENCES [dbo].[ResultTypes]([Id]);
END
GO
