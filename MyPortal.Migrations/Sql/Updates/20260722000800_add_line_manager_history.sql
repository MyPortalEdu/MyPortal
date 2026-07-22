-- Line-manager history. StaffLineManagers is now the source of truth for the reporting line:
-- date-ranged, many per person, a null EndDate being the current manager. Both consumers resolve
-- the row current today — usp_staff_member_is_managed_by (the Managed-scope chain) and
-- usp_staff_member_get_management_by_id — so an ended reporting line no longer grants access
-- indefinitely. StaffMembers.LineManagerId is retained but is no longer authoritative. Existing
-- pointers are backfilled as open-ended rows. Idempotent.

IF OBJECT_ID(N'[dbo].[StaffLineManagers]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffLineManagers] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    [LineManagerId] uniqueidentifier NOT NULL,
    [StartDate] datetime2(7) NOT NULL,
    [EndDate] datetime2(7) NULL,
    [IsDeleted] bit NOT NULL CONSTRAINT DF_StaffLineManagers_IsDeleted DEFAULT (0),
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedByIpAddress] nvarchar(45) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffLineManagers_CreatedAt DEFAULT SYSUTCDATETIME(),
    [LastModifiedById] uniqueidentifier NOT NULL,
    [LastModifiedByIpAddress] nvarchar(45) NOT NULL,
    [LastModifiedAt] datetime2(7) NOT NULL CONSTRAINT DF_StaffLineManagers_LastModifiedAt DEFAULT SYSUTCDATETIME(),
    [Version] BIGINT NOT NULL CONSTRAINT DF_StaffLineManagers_Version DEFAULT (1),
    CONSTRAINT PK_StaffLineManagers PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffLineManagers_StaffMemberId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    ALTER TABLE [dbo].[StaffLineManagers] ADD CONSTRAINT [FK_StaffLineManagers_StaffMemberId_StaffMembers]
        FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffLineManagers_LineManagerId_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    ALTER TABLE [dbo].[StaffLineManagers] ADD CONSTRAINT [FK_StaffLineManagers_LineManagerId_StaffMembers]
        FOREIGN KEY ([LineManagerId]) REFERENCES [dbo].[StaffMembers]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffLineManagers_CreatedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    ALTER TABLE [dbo].[StaffLineManagers] ADD CONSTRAINT [FK_StaffLineManagers_CreatedById_Users]
        FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffLineManagers_LastModifiedById_Users'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    ALTER TABLE [dbo].[StaffLineManagers] ADD CONSTRAINT [FK_StaffLineManagers_LastModifiedById_Users]
        FOREIGN KEY ([LastModifiedById]) REFERENCES [dbo].[Users]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffLineManagers_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    CREATE INDEX [IX_StaffLineManagers_StaffMemberId] ON [dbo].[StaffLineManagers]([StaffMemberId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffLineManagers_LineManagerId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffLineManagers]'))
    CREATE INDEX [IX_StaffLineManagers_LineManagerId] ON [dbo].[StaffLineManagers]([LineManagerId]);
GO

-- ---- Backfill: every existing pointer becomes an open-ended current row ----
-- Guarded on NOT EXISTS so a re-run can't duplicate. The system user is used for the audit
-- stamp because there is no acting user during a migration.
DECLARE @systemUserId UNIQUEIDENTIFIER =
    (SELECT TOP 1 [Id] FROM [dbo].[Users] ORDER BY [CreatedAt]);

IF @systemUserId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[StaffLineManagers]
        ([Id], [StaffMemberId], [LineManagerId], [StartDate], [EndDate], [IsDeleted],
         [CreatedById], [CreatedByIpAddress], [CreatedAt],
         [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version])
    SELECT NEWID(), SM.[Id], SM.[LineManagerId],
           -- Backdated so the historical line reads as "since we started recording it".
           CAST(CAST(SM.[CreatedAt] AS DATE) AS DATETIME2(7)), NULL, 0,
           @systemUserId, N'migration', SYSUTCDATETIME(),
           @systemUserId, N'migration', SYSUTCDATETIME(), 1
    FROM [dbo].[StaffMembers] SM
    WHERE SM.[LineManagerId] IS NOT NULL
      AND SM.[IsDeleted] = 0
      AND NOT EXISTS (
          SELECT 1 FROM [dbo].[StaffLineManagers] X
          WHERE X.[StaffMemberId] = SM.[Id] AND X.[IsDeleted] = 0);
END
GO
