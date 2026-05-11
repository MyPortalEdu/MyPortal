-- Backfill GradeSets audit columns and tighten them to NOT NULL.
-- Previously executed unconditionally on every WebApi boot from AuthSeeder;
-- moved here so schema changes are journalled and run once.
-- Idempotent: no-ops if the columns are already NOT NULL or if there is
-- nothing left to backfill.

IF OBJECT_ID(N'dbo.GradeSets', N'U') IS NOT NULL
BEGIN
    DECLARE @adminId UNIQUEIDENTIFIER = (
        SELECT TOP 1 U.Id
        FROM dbo.Users U
        JOIN dbo.UserRoles UR ON UR.UserId = U.Id
        JOIN dbo.Roles R ON R.Id = UR.RoleId
        WHERE R.NormalizedName = N'SYSTEM ADMINISTRATOR'
        ORDER BY U.CreatedAt
    );

    IF @adminId IS NULL
    BEGIN
        -- Fall back to any system user; if none exists, skip — the AuthSeeder
        -- will create one on next boot and this migration can re-run.
        SELECT TOP 1 @adminId = Id FROM dbo.Users WHERE IsSystem = 1 ORDER BY CreatedAt;
    END

    IF @adminId IS NOT NULL
    BEGIN
        UPDATE dbo.GradeSets
        SET CreatedById      = COALESCE(CreatedById,      @adminId),
            LastModifiedById = COALESCE(LastModifiedById, @adminId)
        WHERE CreatedById IS NULL OR LastModifiedById IS NULL;
    END

    -- Only tighten if there are no remaining NULLs and the column is currently nullable.
    IF NOT EXISTS (SELECT 1 FROM dbo.GradeSets WHERE CreatedById IS NULL)
       AND EXISTS (
            SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID(N'dbo.GradeSets')
              AND name = N'CreatedById'
              AND is_nullable = 1)
    BEGIN
        ALTER TABLE dbo.GradeSets ALTER COLUMN CreatedById UNIQUEIDENTIFIER NOT NULL;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.GradeSets WHERE LastModifiedById IS NULL)
       AND EXISTS (
            SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID(N'dbo.GradeSets')
              AND name = N'LastModifiedById'
              AND is_nullable = 1)
    BEGIN
        ALTER TABLE dbo.GradeSets ALTER COLUMN LastModifiedById UNIQUEIDENTIFIER NOT NULL;
    END
END
GO
