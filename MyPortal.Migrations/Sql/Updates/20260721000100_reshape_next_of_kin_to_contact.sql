-- ============================================================================
-- Emergency Contacts / Next of Kin — repoint the join at a shared Contact.
--
-- The orphaned NextOfKin table linked a staff member to a raw Person and used a
-- bespoke NextOfKinRelationshipTypes lookup. Neither was ever wired up. This
-- reshapes it to mirror StudentContactRelationship: the contact is a shared
-- Contact record (a Person facet), the relationship reuses the shared
-- RelationshipTypes lookup, and a ContactOrder gives call priority. The table
-- gains the standard audit/version/soft-delete columns used by every staff area.
--
-- The old table is empty (no code ever populated it), so a clean drop-and-create
-- is safe and avoids fiddly per-constraint ALTERs. Idempotent throughout.
-- ============================================================================

-- Drop the old shape (its own FKs/indexes go with it).
IF OBJECT_ID(N'[dbo].[NextOfKin]', N'U') IS NOT NULL
    DROP TABLE [dbo].[NextOfKin];
GO

-- Staff next-of-kin uses its own adult-oriented relationship lookup, not the DfE CBDS
-- RelationshipTypes (which is pupil-contact-centric: Childminder, Social Worker, Head Teacher…).
IF OBJECT_ID(N'[dbo].[NextOfKinRelationshipTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[NextOfKinRelationshipTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_NextOfKinRelationshipTypes_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_NextOfKinRelationshipTypes_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_NextOfKinRelationshipTypes_Active DEFAULT (1),
    CONSTRAINT PK_NextOfKinRelationshipTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[NextOfKin]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[NextOfKin] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [ContactId] uniqueidentifier NOT NULL,
    [RelationshipTypeId] uniqueidentifier NULL,
    [ContactOrder] int NOT NULL CONSTRAINT DF_NextOfKin_ContactOrder DEFAULT (0),
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_NextOfKin_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_NextOfKin_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_NextOfKin_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_NextOfKin_Version DEFAULT (1),
    CONSTRAINT PK_NextOfKin PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_StaffMemberId_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_ContactId_Contacts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_ContactId_Contacts]
    FOREIGN KEY ([ContactId]) REFERENCES [dbo].[Contacts]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_RelationshipTypeId_NextOfKinRelationshipTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_RelationshipTypeId_NextOfKinRelationshipTypes]
    FOREIGN KEY ([RelationshipTypeId]) REFERENCES [dbo].[NextOfKinRelationshipTypes]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_CreatedById_Users]
    FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextOfKin_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
ALTER TABLE [dbo].[NextOfKin]
    ADD CONSTRAINT [FK_NextOfKin_LastModifiedById_Users]
    FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_StaffMemberId] ON [dbo].[NextOfKin]([StaffMemberId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_ContactId'
    AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_ContactId] ON [dbo].[NextOfKin]([ContactId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NextOfKin_RelationshipTypeId'
    AND object_id = OBJECT_ID(N'[dbo].[NextOfKin]'))
BEGIN
CREATE INDEX [IX_NextOfKin_RelationshipTypeId] ON [dbo].[NextOfKin]([RelationshipTypeId]);
END
GO

-- ============================================================================
-- Seed the adult next-of-kin relationships. IsSystem so the standard set is
-- protected from edit/delete; "Other Relative" and "Other" pinned last.
-- ============================================================================
MERGE INTO dbo.NextOfKinRelationshipTypes AS Target
USING (VALUES
    (N'A1B2C3D4-0000-4000-8000-000000000001', N'Spouse',         1),
    (N'A1B2C3D4-0000-4000-8000-000000000002', N'Partner',        2),
    (N'A1B2C3D4-0000-4000-8000-000000000003', N'Parent',         3),
    (N'A1B2C3D4-0000-4000-8000-000000000004', N'Sibling',        4),
    (N'A1B2C3D4-0000-4000-8000-000000000005', N'Child',          5),
    (N'A1B2C3D4-0000-4000-8000-000000000006', N'Grandparent',    6),
    (N'A1B2C3D4-0000-4000-8000-000000000007', N'Friend',         7),
    (N'A1B2C3D4-0000-4000-8000-000000000008', N'Neighbour',      8),
    (N'A1B2C3D4-0000-4000-8000-000000000009', N'Carer',          9),
    (N'A1B2C3D4-0000-4000-8000-00000000000A', N'Other Relative', 800),
    (N'A1B2C3D4-0000-4000-8000-00000000000B', N'Other',          801)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO

-- ============================================================================
-- Emergency Contacts is HR-maintained (All scope only), mirroring Pre-Employment
-- Checks. The 20260518000500 collapse deleted the emergency-contacts catalogue,
-- so re-seed the two grantable (All) scopes, then remove the Own/Managed rows any
-- earlier draft may have left behind so the catalogue only advertises All.
-- ============================================================================
MERGE INTO dbo.Permissions AS Target
USING (VALUES
    (N'Staff.ViewAllStaffEmergencyContacts', N'View emergency contacts (all)', N'Staff.EmergencyContacts'),
    (N'Staff.EditAllStaffEmergencyContacts', N'Edit emergency contacts (all)', N'Staff.EmergencyContacts')
) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]
WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName], [Area] = Source.[Area]
WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
GO

DELETE rp
FROM dbo.RolePermissions rp
    INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id
WHERE p.[Name] IN (
    N'Staff.ViewOwnStaffEmergencyContacts',
    N'Staff.ViewManagedStaffEmergencyContacts',
    N'Staff.EditOwnStaffEmergencyContacts',
    N'Staff.EditManagedStaffEmergencyContacts');
GO

DELETE FROM dbo.Permissions
WHERE [Name] IN (
    N'Staff.ViewOwnStaffEmergencyContacts',
    N'Staff.ViewManagedStaffEmergencyContacts',
    N'Staff.EditOwnStaffEmergencyContacts',
    N'Staff.EditManagedStaffEmergencyContacts');
GO
