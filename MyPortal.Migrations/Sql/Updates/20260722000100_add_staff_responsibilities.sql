-- Staff responsibilities — date-ranged designated duties (DSL, first aider, SENCO) held on top
-- of a post. Distinct from the census job role on the contract. Idempotent.

IF OBJECT_ID(N'[dbo].[StaffResponsibilityTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffResponsibilityTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_StaffResponsibilityTypes_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_StaffResponsibilityTypes_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_StaffResponsibilityTypes_Active DEFAULT (1),
    CONSTRAINT PK_StaffResponsibilityTypes PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[StaffResponsibilities]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffResponsibilities] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [ResponsibilityTypeId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [Notes] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffResponsibilities_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffResponsibilities_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffResponsibilities_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffResponsibilities_Version DEFAULT (1),
    CONSTRAINT PK_StaffResponsibilities PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffResponsibilities_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    ALTER TABLE [dbo].[StaffResponsibilities] ADD CONSTRAINT [FK_StaffResponsibilities_StaffMemberId_StaffMembers]
        FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffResponsibilities_ResponsibilityTypeId_StaffResponsibilityTypes'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    ALTER TABLE [dbo].[StaffResponsibilities] ADD CONSTRAINT [FK_StaffResponsibilities_ResponsibilityTypeId_StaffResponsibilityTypes]
        FOREIGN KEY ([ResponsibilityTypeId]) REFERENCES [dbo].[StaffResponsibilityTypes]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffResponsibilities_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    ALTER TABLE [dbo].[StaffResponsibilities] ADD CONSTRAINT [FK_StaffResponsibilities_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffResponsibilities_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    ALTER TABLE [dbo].[StaffResponsibilities] ADD CONSTRAINT [FK_StaffResponsibilities_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffResponsibilities_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    CREATE INDEX [IX_StaffResponsibilities_StaffMemberId] ON [dbo].[StaffResponsibilities]([StaffMemberId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffResponsibilities_ResponsibilityTypeId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffResponsibilities]'))
    CREATE INDEX [IX_StaffResponsibilities_ResponsibilityTypeId] ON [dbo].[StaffResponsibilities]([ResponsibilityTypeId]);
GO

-- ---- Seed the standard responsibilities (system, so protected from edit/delete) ----
MERGE INTO dbo.StaffResponsibilityTypes AS Target
USING (VALUES
    (N'5CA1E000-0000-4000-8000-000000000010', N'Designated Safeguarding Lead (DSL)',    10),
    (N'5CA1E000-0000-4000-8000-000000000020', N'Deputy Safeguarding Lead',              20),
    (N'5CA1E000-0000-4000-8000-000000000030', N'SENCO',                                 30),
    (N'5CA1E000-0000-4000-8000-000000000040', N'First Aider',                           40),
    (N'5CA1E000-0000-4000-8000-000000000050', N'Paediatric First Aider',               50),
    (N'5CA1E000-0000-4000-8000-000000000060', N'Mental Health First Aider',            60),
    (N'5CA1E000-0000-4000-8000-000000000070', N'Fire Marshal / Warden',                70),
    (N'5CA1E000-0000-4000-8000-000000000080', N'Educational Visits Coordinator (EVC)', 80),
    (N'5CA1E000-0000-4000-8000-000000000090', N'Exams Officer',                        90),
    (N'5CA1E000-0000-4000-8000-0000000000A0', N'Data Protection Officer',             100),
    (N'5CA1E000-0000-4000-8000-0000000000B0', N'Prevent Lead',                        110),
    (N'5CA1E000-0000-4000-8000-0000000000C0', N'Designated Teacher for Looked-After Children', 120),
    (N'5CA1E000-0000-4000-8000-0000000000D0', N'Health & Safety Officer',             130),
    (N'5CA1E000-0000-4000-8000-0000000000E0', N'Attendance Lead',                     140)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO

-- ---- Seed the permission catalogue entries ----
MERGE INTO dbo.Permissions AS Target
USING (VALUES
    (N'Staff.ViewOwnStaffResponsibilities',     N'View responsibilities (own)',     N'Staff.Responsibilities'),
    (N'Staff.ViewManagedStaffResponsibilities', N'View responsibilities (managed)', N'Staff.Responsibilities'),
    (N'Staff.ViewAllStaffResponsibilities',     N'View responsibilities (all)',     N'Staff.Responsibilities'),
    (N'Staff.EditAllStaffResponsibilities',     N'Edit responsibilities (all)',     N'Staff.Responsibilities')
) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]
WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName], [Area] = Source.[Area]
WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
GO

-- ---- Self-service baseline: every default staff role can see its own responsibilities ----
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.IsDefault = 1 AND r.UserType = 1
  AND p.[Name] = N'Staff.ViewOwnStaffResponsibilities'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO

-- ---- Grant the HR/Personnel and SLT roles their scopes (mirrors the other HR areas) ----
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    -- HR / Personnel — full maintenance.
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffResponsibilities'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffResponsibilities'),
    -- Senior Leadership Team — view all.
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffResponsibilities')
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
