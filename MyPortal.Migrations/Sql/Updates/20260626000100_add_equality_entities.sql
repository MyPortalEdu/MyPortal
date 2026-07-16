-- ============================================================================
-- Equality & Diversity entities for the staff profile.
--
-- Adds the lookup tables backing the Equality area's single-selects that don't
-- exist yet (Religions, SexualOrientations, GenderIdentities) plus a Disability
-- lookup and the StaffMemberDisabilities join (multi-select). Ethnicity,
-- Nationality, Language and MaritalStatus already exist and are reused.
--
-- The disability join is staff-scoped (the Equality area is staff-only — no
-- Managed scope) and sits alongside the existing StaffMembers.HasDisability /
-- DisabilityDetails declaration + free-text fields.
-- ============================================================================

-- ============================================================================
-- Lookups
-- ============================================================================

IF OBJECT_ID(N'[dbo].[Religions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Religions] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Religions_Active DEFAULT (1),
    CONSTRAINT PK_Religions PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[SexualOrientations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SexualOrientations] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_SexualOrientations_Active DEFAULT (1),
    CONSTRAINT PK_SexualOrientations PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[GenderIdentities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[GenderIdentities] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_GenderIdentities_Active DEFAULT (1),
    CONSTRAINT PK_GenderIdentities PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[Disabilities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Disabilities] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_Disabilities_Active DEFAULT (1),
    CONSTRAINT PK_Disabilities PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ============================================================================
-- StaffMemberDisabilities (multi-select join)
-- ============================================================================

IF OBJECT_ID(N'[dbo].[StaffMemberDisabilities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffMemberDisabilities] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [DisabilityId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_StaffMemberDisabilities PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- One row per (staff member, disability); guards against duplicate declarations.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_StaffMemberDisabilities_StaffMemberId_DisabilityId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffMemberDisabilities]'))
BEGIN
CREATE UNIQUE INDEX [UX_StaffMemberDisabilities_StaffMemberId_DisabilityId]
    ON [dbo].[StaffMemberDisabilities] ([StaffMemberId], [DisabilityId]);
END
GO

-- ============================================================================
-- People: equality additions
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'ReligionId'
)
BEGIN
    ALTER TABLE dbo.People ADD ReligionId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'SexualOrientationId'
)
BEGIN
    ALTER TABLE dbo.People ADD SexualOrientationId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'GenderIdentityId'
)
BEGIN
    ALTER TABLE dbo.People ADD GenderIdentityId uniqueidentifier NULL;
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

-- People --------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_ReligionId_Religions'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_ReligionId_Religions]
    FOREIGN KEY ([ReligionId]) REFERENCES [dbo].[Religions]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_SexualOrientationId_SexualOrientations'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_SexualOrientationId_SexualOrientations]
    FOREIGN KEY ([SexualOrientationId]) REFERENCES [dbo].[SexualOrientations]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_People_GenderIdentityId_GenderIdentities'
    AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
BEGIN
ALTER TABLE [dbo].[People]
    ADD CONSTRAINT [FK_People_GenderIdentityId_GenderIdentities]
    FOREIGN KEY ([GenderIdentityId]) REFERENCES [dbo].[GenderIdentities]([Id]);
END
GO

-- StaffMemberDisabilities ---------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMemberDisabilities_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMemberDisabilities]'))
BEGIN
ALTER TABLE [dbo].[StaffMemberDisabilities]
    ADD CONSTRAINT [FK_StaffMemberDisabilities_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMemberDisabilities_DisabilityId_Disabilities'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMemberDisabilities]'))
BEGIN
ALTER TABLE [dbo].[StaffMemberDisabilities]
    ADD CONSTRAINT [FK_StaffMemberDisabilities_DisabilityId_Disabilities]
    FOREIGN KEY ([DisabilityId]) REFERENCES [dbo].[Disabilities]([Id]);
END
GO
