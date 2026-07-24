-- Established posts and vacancies — the school's staffing structure, which contracts are held
-- against and vacancies reported for. Adds Staff.ViewStaffSetup / EditStaffSetup: school-level
-- data, so these are flat permissions with no per-staff-member scopes. Idempotent.

IF OBJECT_ID(N'[dbo].[PostCategories]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PostCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_PostCategories_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_PostCategories_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_PostCategories_Active DEFAULT (1),
    CONSTRAINT PK_PostCategories PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[Posts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Posts] (
    [Id] uniqueidentifier NOT NULL,
    [Reference] nvarchar(32) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [PostCategoryId] uniqueidentifier NULL,
    [ServiceTermId] uniqueidentifier NULL,
    [SwrPostCode] nvarchar(10) NULL,
    [EstablishedFte] decimal(5,4) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_Posts_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Posts_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Posts_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_Posts_Version DEFAULT (1),
    CONSTRAINT PK_Posts PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[Vacancies]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Vacancies] (
    [Id] uniqueidentifier NOT NULL,
    [PostId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [IsAdvertised] bit NOT NULL CONSTRAINT DF_Vacancies_IsAdvertised DEFAULT (0),
    [IsTemporarilyFilled] bit NOT NULL CONSTRAINT DF_Vacancies_IsTemporarilyFilled DEFAULT (0),
    [SubjectId] uniqueidentifier NULL,
    [Notes] nvarchar(256) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_Vacancies_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_Vacancies_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_Vacancies_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_Vacancies_Version DEFAULT (1),
    CONSTRAINT PK_Vacancies PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_PostCategoryId_PostCategories'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Posts]'))
    ALTER TABLE [dbo].[Posts] ADD CONSTRAINT [FK_Posts_PostCategoryId_PostCategories]
        FOREIGN KEY ([PostCategoryId]) REFERENCES [dbo].[PostCategories]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Posts]'))
    ALTER TABLE [dbo].[Posts] ADD CONSTRAINT [FK_Posts_ServiceTermId_ServiceTerms]
        FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Posts]'))
    ALTER TABLE [dbo].[Posts] ADD CONSTRAINT [FK_Posts_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Posts]'))
    ALTER TABLE [dbo].[Posts] ADD CONSTRAINT [FK_Posts_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Posts_Reference'
    AND object_id = OBJECT_ID(N'[dbo].[Posts]'))
    CREATE INDEX [IX_Posts_Reference] ON [dbo].[Posts]([Reference]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vacancies_PostId_Posts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Vacancies]'))
    ALTER TABLE [dbo].[Vacancies] ADD CONSTRAINT [FK_Vacancies_PostId_Posts]
        FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vacancies_SubjectId_Subjects'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Vacancies]'))
    ALTER TABLE [dbo].[Vacancies] ADD CONSTRAINT [FK_Vacancies_SubjectId_Subjects]
        FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vacancies_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Vacancies]'))
    ALTER TABLE [dbo].[Vacancies] ADD CONSTRAINT [FK_Vacancies_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vacancies_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Vacancies]'))
    ALTER TABLE [dbo].[Vacancies] ADD CONSTRAINT [FK_Vacancies_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vacancies_PostId'
    AND object_id = OBJECT_ID(N'[dbo].[Vacancies]'))
    CREATE INDEX [IX_Vacancies_PostId] ON [dbo].[Vacancies]([PostId]);
GO

-- ---- Contract → Post ----
IF COL_LENGTH(N'[dbo].[StaffContracts]', N'PostId') IS NULL
    ALTER TABLE [dbo].[StaffContracts] ADD [PostId] uniqueidentifier NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffContracts_PostId_Posts'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
    ALTER TABLE [dbo].[StaffContracts] ADD CONSTRAINT [FK_StaffContracts_PostId_Posts]
        FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffContracts_PostId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffContracts]'))
    CREATE INDEX [IX_StaffContracts_PostId] ON [dbo].[StaffContracts]([PostId]);
GO

-- ---- Seeds ----
MERGE INTO dbo.PostCategories AS Target
USING (VALUES
    (N'B057CA70-0000-4000-8000-000000000010', N'Leadership',           10),
    (N'B057CA70-0000-4000-8000-000000000020', N'Teaching',             20),
    (N'B057CA70-0000-4000-8000-000000000030', N'Teaching Assistant',   30),
    (N'B057CA70-0000-4000-8000-000000000040', N'Administration',       40),
    (N'B057CA70-0000-4000-8000-000000000050', N'Technician',           50),
    (N'B057CA70-0000-4000-8000-000000000060', N'Premises & Catering',  60),
    (N'B057CA70-0000-4000-8000-000000000070', N'Other Support',       800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO

-- ---- Permissions ----
MERGE INTO dbo.Permissions AS Target
USING (VALUES
    (N'Staff.ViewStaffSetup', N'View staff setup (posts, pay reference data)', N'Staff.Setup'),
    (N'Staff.EditStaffSetup', N'Edit staff setup (posts, pay reference data)', N'Staff.Setup')
) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]
WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName], [Area] = Source.[Area]
WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
GO

-- HR/Personnel maintains the establishment; SLT can view it.
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewStaffSetup'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditStaffSetup'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewStaffSetup')
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
