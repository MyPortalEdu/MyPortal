-- ============================================================================
-- Contact details plumbing.
--
-- The Addresses / AddressPeople / EmailAddresses / PhoneNumbers tables were
-- scaffolded in the original schema but never brought up to current convention:
-- no soft-delete, no audit columns, and no foreign keys. This migration adds
-- those so the Contact Details feature is built on tables consistent with the
-- rest of the schema.
--
-- Ownership is unchanged: Address is shared (people link via AddressPeople),
-- while EmailAddresses / PhoneNumbers are owned one-to-many off Person (or
-- Agency). This migration only adds plumbing — no relationship changes.
--
-- The audit *ById / *IpAddress columns are added NOT NULL without a default.
-- These tables are dormant (no repos/services ever wrote to them) so they are
-- empty, which is what makes the NOT NULL add safe. Each ALTER is guarded so
-- the script is re-runnable.
-- ============================================================================

-- ============================================================================
-- Soft-delete + audit columns
-- ============================================================================

-- Addresses -----------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Addresses') AND name = N'IsDeleted'
)
BEGIN
ALTER TABLE [dbo].[Addresses] ADD
    [IsDeleted] bit NOT NULL CONSTRAINT DF_Addresses_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Addresses_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Addresses_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_Addresses_Version DEFAULT (1);
END
GO

-- AddressPeople -------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AddressPeople') AND name = N'IsDeleted'
)
BEGIN
ALTER TABLE [dbo].[AddressPeople] ADD
    [IsDeleted] bit NOT NULL CONSTRAINT DF_AddressPeople_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_AddressPeople_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_AddressPeople_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_AddressPeople_Version DEFAULT (1);
END
GO

-- EmailAddresses ------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.EmailAddresses') AND name = N'IsDeleted'
)
BEGIN
ALTER TABLE [dbo].[EmailAddresses] ADD
    [IsDeleted] bit NOT NULL CONSTRAINT DF_EmailAddresses_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_EmailAddresses_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_EmailAddresses_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_EmailAddresses_Version DEFAULT (1);
END
GO

-- PhoneNumbers --------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PhoneNumbers') AND name = N'IsDeleted'
)
BEGIN
ALTER TABLE [dbo].[PhoneNumbers] ADD
    [IsDeleted] bit NOT NULL CONSTRAINT DF_PhoneNumbers_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_PhoneNumbers_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_PhoneNumbers_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_PhoneNumbers_Version DEFAULT (1);
END
GO

-- ============================================================================
-- Foreign keys
-- ============================================================================

-- Addresses (audit only) ----------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Addresses_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Addresses]'))
BEGIN
ALTER TABLE [dbo].[Addresses]
    ADD CONSTRAINT [FK_Addresses_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Addresses_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Addresses]'))
BEGIN
ALTER TABLE [dbo].[Addresses]
    ADD CONSTRAINT [FK_Addresses_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- AddressPeople -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_AddressId_Addresses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_AddressId_Addresses]
    FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Addresses]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_PersonId_People'
    AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_PersonId_People]
    FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_AddressTypeId_AddressTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_AddressTypeId_AddressTypes]
    FOREIGN KEY ([AddressTypeId]) REFERENCES [dbo].[AddressTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AddressPeople_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
ALTER TABLE [dbo].[AddressPeople]
    ADD CONSTRAINT [FK_AddressPeople_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- EmailAddresses ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_TypeId_EmailAddressTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_TypeId_EmailAddressTypes]
    FOREIGN KEY ([TypeId]) REFERENCES [dbo].[EmailAddressTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_PersonId_People'
    AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_PersonId_People]
    FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_AgencyId_Agencies'
    AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_AgencyId_Agencies]
    FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailAddresses_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
ALTER TABLE [dbo].[EmailAddresses]
    ADD CONSTRAINT [FK_EmailAddresses_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- PhoneNumbers --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_TypeId_PhoneNumberTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_TypeId_PhoneNumberTypes]
    FOREIGN KEY ([TypeId]) REFERENCES [dbo].[PhoneNumberTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_PersonId_People'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_PersonId_People]
    FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_AgencyId_Agencies'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_AgencyId_Agencies]
    FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PhoneNumbers_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
ALTER TABLE [dbo].[PhoneNumbers]
    ADD CONSTRAINT [FK_PhoneNumbers_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

-- ============================================================================
-- Indexes (FK / filter columns)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressPeople_PersonId'
    AND object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
CREATE INDEX [IX_AddressPeople_PersonId] ON [dbo].[AddressPeople]([PersonId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AddressPeople_AddressId'
    AND object_id = OBJECT_ID(N'[dbo].[AddressPeople]'))
BEGIN
CREATE INDEX [IX_AddressPeople_AddressId] ON [dbo].[AddressPeople]([AddressId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAddresses_PersonId'
    AND object_id = OBJECT_ID(N'[dbo].[EmailAddresses]'))
BEGIN
CREATE INDEX [IX_EmailAddresses_PersonId] ON [dbo].[EmailAddresses]([PersonId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhoneNumbers_PersonId'
    AND object_id = OBJECT_ID(N'[dbo].[PhoneNumbers]'))
BEGIN
CREATE INDEX [IX_PhoneNumbers_PersonId] ON [dbo].[PhoneNumbers]([PersonId]);
END
GO
