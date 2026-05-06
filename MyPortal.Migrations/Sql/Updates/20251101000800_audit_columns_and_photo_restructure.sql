-- Bring existing databases in line with db_create_tables.sql changes from
-- the school-bulletins-api branch:
--   * Adds full audit blocks (CreatedBy*/LastModifiedBy*/Version) to tables that lacked them.
--   * Adds Version column to tables that already had the audit block.
--   * Restructures Photos: drops Data/MimeType, adds DocumentId + FK to Documents.
--
-- Pre-prod assumption: target tables are empty. The defensive empty-table checks
-- below fail loudly with a helpful message if rows exist.

------------------------------------------------------------------------
-- Section 1: full audit + Version added (CreatedById/LastModifiedById NOT NULL)
------------------------------------------------------------------------

DECLARE @table SYSNAME, @sql NVARCHAR(MAX), @notice NVARCHAR(400);

DECLARE notNullAudit CURSOR LOCAL FAST_FORWARD FOR
SELECT t FROM (VALUES
    ('AcademicTerms'),
    ('AcademicYears'),
    ('BuildingFloors'),
    ('Buildings'),
    ('Classes'),
    ('Courses'),
    ('RegGroups'),
    ('ResultSets'),
    ('Rooms'),
    ('StaffMembers'),
    ('StudentGroups'),
    ('Students'),
    ('Subjects'),
    ('YearGroups')
) AS x(t);

OPEN notNullAudit;
FETCH NEXT FROM notNullAudit INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.' + @table)
          AND name = N'CreatedById'
    )
    BEGIN
        SET @sql = N'IF EXISTS (SELECT TOP 1 1 FROM dbo.' + QUOTENAME(@table) + N')
                     BEGIN
                         ;THROW 50010, ''Update 0009 cannot run: dbo.' + @table + N' has existing rows but no audit columns. Wipe the table or backfill audit data manually before applying.'', 1;
                     END;
                     ALTER TABLE dbo.' + QUOTENAME(@table) + N'
                         ADD [CreatedById] UNIQUEIDENTIFIER NOT NULL,
                             [CreatedByIpAddress] NVARCHAR(45) NOT NULL,
                             [CreatedAt] DATETIME2(7) NOT NULL,
                             [LastModifiedById] UNIQUEIDENTIFIER NOT NULL,
                             [LastModifiedByIpAddress] NVARCHAR(45) NOT NULL,
                             [LastModifiedAt] DATETIME2(7) NOT NULL,
                             [Version] BIGINT NOT NULL CONSTRAINT DF_' + @table + N'_Version DEFAULT (1);';
        EXEC sp_executesql @sql;
    END;
    FETCH NEXT FROM notNullAudit INTO @table;
END;
CLOSE notNullAudit;
DEALLOCATE notNullAudit;
GO

------------------------------------------------------------------------
-- Section 2: full audit + Version added (CreatedById/LastModifiedById NULLABLE)
------------------------------------------------------------------------

DECLARE @table SYSNAME, @sql NVARCHAR(MAX);

DECLARE nullableAudit CURSOR LOCAL FAST_FORWARD FOR
SELECT t FROM (VALUES
    ('Directories'),
    ('GradeSets')
) AS x(t);

OPEN nullableAudit;
FETCH NEXT FROM nullableAudit INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.' + @table)
          AND name = N'CreatedById'
    )
    BEGIN
        SET @sql = N'IF EXISTS (SELECT TOP 1 1 FROM dbo.' + QUOTENAME(@table) + N')
                     BEGIN
                         ;THROW 50011, ''Update 0009 cannot run: dbo.' + @table + N' has existing rows but no audit columns. Wipe the table or backfill audit data manually before applying.'', 1;
                     END;
                     ALTER TABLE dbo.' + QUOTENAME(@table) + N'
                         ADD [CreatedById] UNIQUEIDENTIFIER NULL,
                             [CreatedByIpAddress] NVARCHAR(45) NOT NULL,
                             [CreatedAt] DATETIME2(7) NOT NULL,
                             [LastModifiedById] UNIQUEIDENTIFIER NULL,
                             [LastModifiedByIpAddress] NVARCHAR(45) NOT NULL,
                             [LastModifiedAt] DATETIME2(7) NOT NULL,
                             [Version] BIGINT NOT NULL CONSTRAINT DF_' + @table + N'_Version DEFAULT (1);';
        EXEC sp_executesql @sql;
    END;
    FETCH NEXT FROM nullableAudit INTO @table;
END;
CLOSE nullableAudit;
DEALLOCATE nullableAudit;
GO

------------------------------------------------------------------------
-- Section 3: Users (CreatedAt already existed; only the rest of the audit block + Version are new)
------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'CreatedById'
)
BEGIN
    IF EXISTS (SELECT TOP 1 1 FROM dbo.Users)
    BEGIN
        ;THROW 50012, 'Update 0009 cannot run: dbo.Users has existing rows but missing audit columns. Wipe the table or backfill audit data manually before applying.', 1;
    END

    ALTER TABLE dbo.Users
        ADD [CreatedById] UNIQUEIDENTIFIER NULL,
            [CreatedByIpAddress] NVARCHAR(45) NOT NULL,
            [LastModifiedById] UNIQUEIDENTIFIER NULL,
            [LastModifiedByIpAddress] NVARCHAR(45) NOT NULL,
            [LastModifiedAt] DATETIME2(7) NOT NULL,
            [Version] BIGINT NOT NULL CONSTRAINT DF_Users_Version DEFAULT (1);
END
GO

------------------------------------------------------------------------
-- Section 4: Version column only (audit block already present)
-- Safe even on populated tables thanks to DEFAULT (1).
------------------------------------------------------------------------

DECLARE @table SYSNAME, @sql NVARCHAR(MAX);

DECLARE versionOnly CURSOR LOCAL FAST_FORWARD FOR
SELECT t FROM (VALUES
    ('Achievements'),
    ('Bills'),
    ('Bulletins'),
    ('DiaryEvents'),
    ('Documents'),
    ('Incidents'),
    ('LessonPlans'),
    ('LogNotes'),
    ('MedicalEvents'),
    ('People'),
    ('Results'),
    ('Tasks')
) AS x(t);

OPEN versionOnly;
FETCH NEXT FROM versionOnly INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.' + @table)
          AND name = N'Version'
    )
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@table) + N'
                         ADD [Version] BIGINT NOT NULL CONSTRAINT DF_' + @table + N'_Version DEFAULT (1);';
        EXEC sp_executesql @sql;
    END;
    FETCH NEXT FROM versionOnly INTO @table;
END;
CLOSE versionOnly;
DEALLOCATE versionOnly;
GO

------------------------------------------------------------------------
-- Section 5: Photos restructure
--   - Drop legacy [Data] varbinary(max) and [MimeType]
--   - Add [DocumentId] UNIQUEIDENTIFIER NOT NULL + FK to Documents
--   - Add [Version]
------------------------------------------------------------------------

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Photos') AND name = N'Data'
)
BEGIN
    IF EXISTS (SELECT TOP 1 1 FROM dbo.Photos)
    BEGIN
        ;THROW 50013, 'Update 0009 cannot run: dbo.Photos has existing rows. Migrate the binary data into dbo.Documents and populate Photos.DocumentId before applying.', 1;
    END

    ALTER TABLE dbo.Photos DROP COLUMN [Data];
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Photos') AND name = N'MimeType'
)
BEGIN
    ALTER TABLE dbo.Photos DROP COLUMN [MimeType];
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Photos') AND name = N'DocumentId'
)
BEGIN
    ALTER TABLE dbo.Photos
        ADD [DocumentId] UNIQUEIDENTIFIER NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Photos') AND name = N'Version'
)
BEGIN
    ALTER TABLE dbo.Photos
        ADD [Version] BIGINT NOT NULL CONSTRAINT DF_Photos_Version DEFAULT (1);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Photos_DocumentId_Documents'
      AND parent_object_id = OBJECT_ID(N'dbo.Photos')
)
BEGIN
    ALTER TABLE dbo.Photos
        ADD CONSTRAINT FK_Photos_DocumentId_Documents
            FOREIGN KEY (DocumentId)
                REFERENCES dbo.Documents(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Photos_DocumentId'
      AND object_id = OBJECT_ID(N'dbo.Photos')
)
BEGIN
    CREATE INDEX IX_Photos_DocumentId ON dbo.Photos(DocumentId);
END
GO
