-- Seeds a sentinel "system" user (fixed Guid, cannot log in) so subsequent seed
-- migrations have a real dbo.Users.Id to reference for CreatedById / LastModifiedById
-- and ownership of system rows. Without this row, fresh-database deploys break on
-- the first migration that audits its inserts (BulletinCategories, etc.) — the
-- old workaround was "run migrations → run WebApi to seed admin → run migrations
-- again", which is awful onboarding.
--
-- Slots between 20251101000000_add_identity (which adds the Identity columns to
-- dbo.Users) and 20251101000100_add_user_management_permissions (the first seed
-- migration that doesn't need ownership). Audit columns on dbo.Users are
-- already present from the baseline (db_create_tables.sql) so we can populate
-- them straight away.
--
-- Idempotent: skips if a row with the sentinel Id already exists. Safe to
-- re-apply if the journal is ever lost.
--
-- Mirrored in C# by MyPortal.Common.Constants.SystemUsers.SentinelUserId
-- ('00000000-0000-0000-0000-000000000001'). Keep the two in sync.

DECLARE @SystemUserId UNIQUEIDENTIFIER = N'00000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @SystemUserId)
BEGIN
    INSERT INTO dbo.Users (
        [Id],
        [PersonId],
        [UserType],
        [IsEnabled],
        [IsSystem],
        -- Audit columns (from baseline). CreatedById/LastModifiedById are nullable;
        -- leaving them NULL is the cleanest "this row was created by the system,
        -- no real principal" signal. The IpAddress / At columns are NOT NULL.
        [CreatedById],
        [CreatedByIpAddress],
        [CreatedAt],
        [LastModifiedById],
        [LastModifiedByIpAddress],
        [LastModifiedAt],
        -- Identity columns (added by 20251101000000_add_identity.sql).
        -- UserName / NormalizedUserName are required so Identity's APIs don't
        -- choke on the row if anyone ever calls UserManager.FindBy*; everything
        -- else stays at the column defaults (EmailConfirmed=0, LockoutEnabled=1,
        -- PhoneNumberConfirmed=0, AccessFailedCount=0, TwoFactorEnabled=0).
        [UserName],
        [NormalizedUserName]
    )
    VALUES (
        @SystemUserId,
        NULL,
        1,                          -- UserType.Staff (1 — see MyPortal.Common.Enums.UserType)
        0,                          -- IsEnabled = 0: cannot log in. Belt-and-braces with no PasswordHash.
        1,                          -- IsSystem = 1
        NULL,                       -- CreatedById
        N'::1',
        SYSUTCDATETIME(),
        NULL,                       -- LastModifiedById
        N'::1',
        SYSUTCDATETIME(),
        N'_system',
        N'_SYSTEM'
    );
END;
GO
