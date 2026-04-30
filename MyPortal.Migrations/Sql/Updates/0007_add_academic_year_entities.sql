-- StudentGroups -----------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StudentGroups') AND name = N'AcademicYearId'
)
BEGIN
    ALTER TABLE dbo.StudentGroups
        ADD AcademicYearId UNIQUEIDENTIFIER NULL;
END
GO

DECLARE @defaultAcademicYearId UNIQUEIDENTIFIER;
SELECT TOP 1 @defaultAcademicYearId = Id FROM dbo.AcademicYears ORDER BY Name;

IF @defaultAcademicYearId IS NULL
   AND EXISTS (SELECT 1 FROM dbo.StudentGroups WHERE AcademicYearId IS NULL)
BEGIN
    ;THROW 50001, 'Update 0007 cannot run: dbo.StudentGroups has existing rows but dbo.AcademicYears is empty. Seed at least one academic year before applying.', 1;
END

UPDATE dbo.StudentGroups
SET AcademicYearId = @defaultAcademicYearId
WHERE AcademicYearId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StudentGroups')
      AND name = N'AcademicYearId'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.StudentGroups
        ALTER COLUMN AcademicYearId UNIQUEIDENTIFIER NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_StudentGroups_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.StudentGroups')
)
BEGIN
    ALTER TABLE dbo.StudentGroups
        ADD CONSTRAINT FK_StudentGroups_AcademicYears
            FOREIGN KEY (AcademicYearId)
                REFERENCES dbo.AcademicYears(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_StudentGroups_AcademicYearId'
      AND object_id = OBJECT_ID(N'dbo.StudentGroups')
)
BEGIN
    CREATE INDEX IX_StudentGroups_AcademicYearId
        ON dbo.StudentGroups(AcademicYearId);
END
GO

-- ReportCards -------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ReportCards') AND name = N'AcademicYearId'
)
BEGIN
    ALTER TABLE dbo.ReportCards
        ADD AcademicYearId UNIQUEIDENTIFIER NULL;
END
GO

DECLARE @defaultAcademicYearId UNIQUEIDENTIFIER;
SELECT TOP 1 @defaultAcademicYearId = Id FROM dbo.AcademicYears ORDER BY Name;

IF @defaultAcademicYearId IS NULL
   AND EXISTS (SELECT 1 FROM dbo.ReportCards WHERE AcademicYearId IS NULL)
BEGIN
    ;THROW 50002, 'Update 0007 cannot run: dbo.ReportCards has existing rows but dbo.AcademicYears is empty. Seed at least one academic year before applying.', 1;
END

UPDATE dbo.ReportCards
SET AcademicYearId = @defaultAcademicYearId
WHERE AcademicYearId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ReportCards')
      AND name = N'AcademicYearId'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.ReportCards
        ALTER COLUMN AcademicYearId UNIQUEIDENTIFIER NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_ReportCards_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.ReportCards')
)
BEGIN
    ALTER TABLE dbo.ReportCards
        ADD CONSTRAINT FK_ReportCards_AcademicYears
            FOREIGN KEY (AcademicYearId)
                REFERENCES dbo.AcademicYears(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ReportCards_AcademicYearId'
      AND object_id = OBJECT_ID(N'dbo.ReportCards')
)
BEGIN
    CREATE INDEX IX_ReportCards_AcademicYearId
        ON dbo.ReportCards(AcademicYearId);
END
GO
