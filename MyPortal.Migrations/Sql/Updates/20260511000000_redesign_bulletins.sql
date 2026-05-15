-- Bulletins redesign: categories, audience targeting, pinning (replaces approval),
-- and per-user acknowledgements. See feature spec for shape.
--
-- Order matters: seed categories before backfilling Bulletins.CategoryId, then make
-- the FK column NOT NULL, then drop IsApproved. Permission rename happens last so
-- any RolePermissions rows referencing the old ApproveSchoolBulletins are removed
-- in the same script.

-- ─── 1. BulletinCategories ──────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[BulletinCategories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BulletinCategories] (
        [Id]                       UNIQUEIDENTIFIER NOT NULL,
        [Name]                     NVARCHAR(50)     NOT NULL,
        [Icon]                     NVARCHAR(50)     NOT NULL,
        [ColourCode]               NVARCHAR(9)      NOT NULL,  -- '#RRGGBB' or '#RRGGBBAA'
        [DisplayOrder]             INT              NOT NULL CONSTRAINT DF_BulletinCategories_DisplayOrder DEFAULT (0),
        [Active]                   BIT              NOT NULL CONSTRAINT DF_BulletinCategories_Active       DEFAULT (1),
        [IsSystem]                 BIT              NOT NULL CONSTRAINT DF_BulletinCategories_IsSystem     DEFAULT (0),
        [CreatedById]              UNIQUEIDENTIFIER NOT NULL,
        [CreatedByIpAddress]       NVARCHAR(45)     NOT NULL,
        [CreatedAt]                DATETIME2(7)     NOT NULL CONSTRAINT DF_BulletinCategories_CreatedAt      DEFAULT SYSUTCDATETIME(),
        [LastModifiedById]         UNIQUEIDENTIFIER NOT NULL,
        [LastModifiedByIpAddress]  NVARCHAR(45)     NOT NULL,
        [LastModifiedAt]           DATETIME2(7)     NOT NULL CONSTRAINT DF_BulletinCategories_LastModifiedAt DEFAULT SYSUTCDATETIME(),
        [Version]                  BIGINT           NOT NULL CONSTRAINT DF_BulletinCategories_Version        DEFAULT (1),
        CONSTRAINT PK_BulletinCategories PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT UQ_BulletinCategories_Name UNIQUE ([Name])
    );
END
GO

-- ─── 2. Seed default system categories ──────────────────────────────────────
-- Attribute to the sentinel system user (seeded by 20251101000050_seed_system_user.sql;
-- mirrored in MyPortal.Common.Constants.SystemUsers.SentinelUserId). Direct Guid
-- reference is deterministic — earlier versions of this script did
-- "WHERE IsSystem = 1 ORDER BY CreatedAt" which silently picked the wrong row
-- if AuthSeeder ran before the sentinel migration on a deployed environment.
DECLARE @sysUser UNIQUEIDENTIFIER = N'00000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @sysUser)
    THROW 50000,
        N'Cannot seed BulletinCategories: sentinel system user (00000000-0000-0000-0000-000000000001) is missing. Migration 20251101000050_seed_system_user.sql should have created it.',
        1;

MERGE INTO dbo.BulletinCategories AS T
USING (VALUES
    (N'Notice',       N'fa-regular fa-megaphone',     N'#6366F1', 10),
    (N'Safeguarding', N'fa-regular fa-shield-halved', N'#EF4444', 20),
    (N'Event',        N'fa-regular fa-calendar',      N'#10B981', 30),
    (N'Reminder',     N'fa-regular fa-bell',          N'#F59E0B', 40),
    (N'Celebration',  N'fa-regular fa-star',          N'#EC4899', 50)
) AS S ([Name], Icon, ColourCode, DisplayOrder)
ON T.[Name] = S.[Name]
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, [Name], Icon, ColourCode, DisplayOrder, Active, IsSystem,
            CreatedById, CreatedByIpAddress, CreatedAt,
            LastModifiedById, LastModifiedByIpAddress, LastModifiedAt, Version)
    -- Seed defaults as user-managed (IsSystem = 0): the spec says seeded
    -- categories must be customisable, so we don't want EntityRepository's
    -- system-entity guard to block edits / deletes.
    VALUES (NEWID(), S.[Name], S.Icon, S.ColourCode, S.DisplayOrder, 1, 0,
            @sysUser, N'::1', SYSUTCDATETIME(),
            @sysUser, N'::1', SYSUTCDATETIME(), 1);
GO

-- ─── 3. Bulletins: add new columns ──────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bulletins') AND name = N'CategoryId')
BEGIN
    ALTER TABLE dbo.Bulletins ADD [CategoryId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bulletins') AND name = N'PinnedAt')
BEGIN
    ALTER TABLE dbo.Bulletins ADD [PinnedAt] DATETIME2(7) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bulletins') AND name = N'RequiresAcknowledgement')
BEGIN
    ALTER TABLE dbo.Bulletins
        ADD [RequiresAcknowledgement] BIT NOT NULL
            CONSTRAINT DF_Bulletins_RequiresAcknowledgement DEFAULT (0);
END
GO

-- Backfill: every existing bulletin defaults to the Notice category.
DECLARE @noticeId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.BulletinCategories WHERE [Name] = N'Notice');
UPDATE dbo.Bulletins SET CategoryId = @noticeId WHERE CategoryId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Bulletins')
      AND name = N'CategoryId'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Bulletins ALTER COLUMN [CategoryId] UNIQUEIDENTIFIER NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bulletins_CategoryId_BulletinCategories')
BEGIN
    ALTER TABLE dbo.Bulletins
        ADD CONSTRAINT FK_Bulletins_CategoryId_BulletinCategories
            FOREIGN KEY ([CategoryId]) REFERENCES dbo.BulletinCategories([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bulletins_CategoryId' AND object_id = OBJECT_ID(N'dbo.Bulletins'))
BEGIN
    CREATE INDEX IX_Bulletins_CategoryId ON dbo.Bulletins([CategoryId]);
END
GO

-- ─── 4. Drop IsApproved and IsPrivate ───────────────────────────────────────
-- usp_bulletin_get_details_by_id and GetBulletinSummaries.sql reference these
-- columns; the StoredProcedures pass runs AFTER all Updates, so the SP is
-- re-created with the new definition before any caller hits it.
-- GetBulletinSummaries.sql is loaded at runtime from MyPortal.Data — the
-- corresponding C# change ships in the same release.
--
-- IsPrivate is removed because audience targeting now provides the gating
-- ("All Staff" makes a bulletin staff-only; etc.). One concept, not two.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bulletins') AND name = N'IsApproved')
BEGIN
    DECLARE @dfApproved SYSNAME;
    SELECT @dfApproved = dc.[name]
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE c.object_id = OBJECT_ID(N'dbo.Bulletins') AND c.[name] = N'IsApproved';
    IF @dfApproved IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Bulletins DROP CONSTRAINT [' + @dfApproved + N'];');

    ALTER TABLE dbo.Bulletins DROP COLUMN [IsApproved];
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bulletins') AND name = N'IsPrivate')
BEGIN
    DECLARE @dfPrivate SYSNAME;
    SELECT @dfPrivate = dc.[name]
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE c.object_id = OBJECT_ID(N'dbo.Bulletins') AND c.[name] = N'IsPrivate';
    IF @dfPrivate IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Bulletins DROP CONSTRAINT [' + @dfPrivate + N'];');

    ALTER TABLE dbo.Bulletins DROP COLUMN [IsPrivate];
END
GO

-- ─── 5. BulletinAudiences ───────────────────────────────────────────────────
-- AudienceKind: 1 = AllStaff, 2 = AllPupils, 3 = AllParents, 4 = StudentGroup.
-- The check constraint enforces that StudentGroupId is set iff kind = 4.
IF OBJECT_ID(N'[dbo].[BulletinAudiences]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BulletinAudiences] (
        [Id]             UNIQUEIDENTIFIER NOT NULL,
        [BulletinId]     UNIQUEIDENTIFIER NOT NULL,
        [AudienceKind]   TINYINT          NOT NULL,
        [StudentGroupId] UNIQUEIDENTIFIER NULL,
        CONSTRAINT PK_BulletinAudiences PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT CK_BulletinAudiences_StudentGroupRequired
            CHECK ((AudienceKind = 4 AND StudentGroupId IS NOT NULL)
                OR (AudienceKind IN (1, 2, 3) AND StudentGroupId IS NULL))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BulletinAudiences_BulletinId_Bulletins')
BEGIN
    ALTER TABLE dbo.BulletinAudiences
        ADD CONSTRAINT FK_BulletinAudiences_BulletinId_Bulletins
            FOREIGN KEY ([BulletinId]) REFERENCES dbo.Bulletins([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BulletinAudiences_StudentGroupId_StudentGroups')
BEGIN
    ALTER TABLE dbo.BulletinAudiences
        ADD CONSTRAINT FK_BulletinAudiences_StudentGroupId_StudentGroups
            FOREIGN KEY ([StudentGroupId]) REFERENCES dbo.StudentGroups([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BulletinAudiences_BulletinId' AND object_id = OBJECT_ID(N'dbo.BulletinAudiences'))
BEGIN
    CREATE INDEX IX_BulletinAudiences_BulletinId ON dbo.BulletinAudiences([BulletinId]);
END
GO

-- Two partial unique indexes prevent duplicate audience targets per bulletin.
-- Splitting them on the StudentGroupId predicate is the only way to express
-- "unique by (BulletinId, AudienceKind, StudentGroupId) when group is set,
-- unique by (BulletinId, AudienceKind) when group is null" in SQL Server.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_BulletinAudiences_BulletinKind_Group' AND object_id = OBJECT_ID(N'dbo.BulletinAudiences'))
BEGIN
    CREATE UNIQUE INDEX UQ_BulletinAudiences_BulletinKind_Group
        ON dbo.BulletinAudiences([BulletinId], [AudienceKind], [StudentGroupId])
        WHERE [StudentGroupId] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_BulletinAudiences_BulletinKind' AND object_id = OBJECT_ID(N'dbo.BulletinAudiences'))
BEGIN
    CREATE UNIQUE INDEX UQ_BulletinAudiences_BulletinKind
        ON dbo.BulletinAudiences([BulletinId], [AudienceKind])
        WHERE [StudentGroupId] IS NULL;
END
GO

-- ─── 6. BulletinAudienceAllowedGroups ──────────────────────────────────────
-- Allowlist of StudentGroups that may be picked as a bulletin audience. Managed
-- via bulletin settings. Keeping it separate from StudentGroups means the
-- general group table doesn't carry a bulletin-specific flag.
IF OBJECT_ID(N'[dbo].[BulletinAudienceAllowedGroups]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BulletinAudienceAllowedGroups] (
        [StudentGroupId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_BulletinAudienceAllowedGroups PRIMARY KEY CLUSTERED ([StudentGroupId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BulletinAudienceAllowedGroups_StudentGroupId_StudentGroups')
BEGIN
    ALTER TABLE dbo.BulletinAudienceAllowedGroups
        ADD CONSTRAINT FK_BulletinAudienceAllowedGroups_StudentGroupId_StudentGroups
            FOREIGN KEY ([StudentGroupId]) REFERENCES dbo.StudentGroups([Id]) ON DELETE CASCADE;
END
GO

-- ─── 7. BulletinAcknowledgements ────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[BulletinAcknowledgements]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BulletinAcknowledgements] (
        [Id]             UNIQUEIDENTIFIER NOT NULL,
        [BulletinId]     UNIQUEIDENTIFIER NOT NULL,
        [UserId]         UNIQUEIDENTIFIER NOT NULL,
        [AcknowledgedAt] DATETIME2(7)     NOT NULL
            CONSTRAINT DF_BulletinAcknowledgements_AcknowledgedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_BulletinAcknowledgements PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT UQ_BulletinAcknowledgements_BulletinUser UNIQUE ([BulletinId], [UserId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BulletinAcknowledgements_BulletinId_Bulletins')
BEGIN
    ALTER TABLE dbo.BulletinAcknowledgements
        ADD CONSTRAINT FK_BulletinAcknowledgements_BulletinId_Bulletins
            FOREIGN KEY ([BulletinId]) REFERENCES dbo.Bulletins([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BulletinAcknowledgements_UserId_Users')
BEGIN
    ALTER TABLE dbo.BulletinAcknowledgements
        ADD CONSTRAINT FK_BulletinAcknowledgements_UserId_Users
            FOREIGN KEY ([UserId]) REFERENCES dbo.Users([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BulletinAcknowledgements_BulletinId' AND object_id = OBJECT_ID(N'dbo.BulletinAcknowledgements'))
BEGIN
    CREATE INDEX IX_BulletinAcknowledgements_BulletinId ON dbo.BulletinAcknowledgements([BulletinId]);
END
GO

-- ─── 8. Permissions: replace ApproveSchoolBulletins with PinSchoolBulletins ─
DECLARE @approveId UNIQUEIDENTIFIER =
    (SELECT Id FROM dbo.Permissions WHERE [Name] = N'School.ApproveSchoolBulletins');

IF @approveId IS NOT NULL
BEGIN
    DELETE FROM dbo.RolePermissions WHERE PermissionId = @approveId;
    DELETE FROM dbo.Permissions     WHERE Id           = @approveId;
END

MERGE INTO dbo.Permissions AS T
USING (VALUES
    (N'School.PinSchoolBulletins', N'Pin School Bulletins', N'School.Bulletins')
) AS S ([Name], FriendlyName, Area)
ON T.[Name] = S.[Name]
WHEN MATCHED AND (T.FriendlyName <> S.FriendlyName OR T.Area <> S.Area) THEN
    UPDATE SET FriendlyName = S.FriendlyName, Area = S.Area
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, [Name], FriendlyName, Area)
    VALUES (NEWID(), S.[Name], S.FriendlyName, S.Area);
GO
