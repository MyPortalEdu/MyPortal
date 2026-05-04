-- ============================================================================
-- Collapse AttendanceWeekPattern into AcademicYear.
--
-- AttendanceWeekPattern was a 1:1 child of AcademicYear carrying nothing but a
-- description and CycleLength. Move CycleLength onto AcademicYear and reparent
-- AttendancePeriod directly to AcademicYear; drop the now-empty pattern table.
-- ============================================================================

IF OBJECT_ID(N'dbo.vw_session_period_metadata', N'V') IS NOT NULL
    DROP VIEW dbo.vw_session_period_metadata;
GO
IF OBJECT_ID(N'dbo.vw_attendance_period_instances', N'V') IS NOT NULL
    DROP VIEW dbo.vw_attendance_period_instances;
GO

-- ─── AcademicYears: + TimetableCycleLength ──────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AcademicYears') AND name = N'TimetableCycleLength'
)
BEGIN
    ALTER TABLE dbo.AcademicYears
        ADD TimetableCycleLength INT NOT NULL
            CONSTRAINT DF_AcademicYears_TimetableCycleLength DEFAULT (5);
END
GO

-- Backfill from any existing pattern. AWP→AY was 1:1, so a single UPDATE is safe.
IF OBJECT_ID(N'dbo.AttendanceWeekPatterns', N'U') IS NOT NULL
BEGIN
    UPDATE AY
    SET AY.TimetableCycleLength = AWP.CycleLength
    FROM dbo.AcademicYears AS AY
    JOIN dbo.AttendanceWeekPatterns AS AWP ON AWP.AcademicYearId = AY.Id;
END
GO

-- ─── AttendancePeriods: WeekPatternId → AcademicYearId ──────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'AcademicYearId'
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods ADD AcademicYearId UNIQUEIDENTIFIER NULL;
END
GO

-- Backfill AcademicYearId by following the (now-disappearing) pattern FK.
IF OBJECT_ID(N'dbo.AttendanceWeekPatterns', N'U') IS NOT NULL
   AND EXISTS (
       SELECT 1 FROM sys.columns
       WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'WeekPatternId'
   )
BEGIN
    UPDATE AP
    SET AP.AcademicYearId = AWP.AcademicYearId
    FROM dbo.AttendancePeriods AS AP
    JOIN dbo.AttendanceWeekPatterns AS AWP ON AWP.Id = AP.WeekPatternId
    WHERE AP.AcademicYearId IS NULL;
END
GO

-- AttendancePeriods rows that survived without a pattern (shouldn't exist, but defend
-- against pre-existing orphans) get the default AY so the NOT NULL flip can succeed.
DECLARE @defaultAcademicYearId UNIQUEIDENTIFIER;
SELECT TOP 1 @defaultAcademicYearId = Id FROM dbo.AcademicYears ORDER BY Name;

IF @defaultAcademicYearId IS NULL
   AND EXISTS (SELECT 1 FROM dbo.AttendancePeriods WHERE AcademicYearId IS NULL)
BEGIN
    ;THROW 50019, 'Update 0019 cannot run: dbo.AttendancePeriods has rows without an AcademicYearId and dbo.AcademicYears is empty. Seed at least one academic year before applying.', 1;
END

UPDATE dbo.AttendancePeriods
SET AcademicYearId = @defaultAcademicYearId
WHERE AcademicYearId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods')
      AND name = N'AcademicYearId' AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods ALTER COLUMN AcademicYearId UNIQUEIDENTIFIER NOT NULL;
END
GO

-- Drop the old WeekPatternId FK + index + column.
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_AttendancePeriods_WeekPatternId_AttendanceWeekPatterns'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods
        DROP CONSTRAINT FK_AttendancePeriods_WeekPatternId_AttendanceWeekPatterns;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_AttendancePeriods_WeekPatternId'
      AND object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    DROP INDEX IX_AttendancePeriods_WeekPatternId ON dbo.AttendancePeriods;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'WeekPatternId'
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods DROP COLUMN WeekPatternId;
END
GO

-- New FK + index on AcademicYearId.
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_AttendancePeriods_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods
        ADD CONSTRAINT FK_AttendancePeriods_AcademicYears
            FOREIGN KEY (AcademicYearId) REFERENCES dbo.AcademicYears(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_AttendancePeriods_AcademicYearId'
      AND object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    CREATE INDEX IX_AttendancePeriods_AcademicYearId
        ON dbo.AttendancePeriods(AcademicYearId);
END
GO

-- ─── Drop AttendanceWeekPatterns ────────────────────────────────────────────

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_AttendanceWeekPatterns_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns')
)
BEGIN
    ALTER TABLE dbo.AttendanceWeekPatterns
        DROP CONSTRAINT FK_AttendanceWeekPatterns_AcademicYears;
END
GO

IF OBJECT_ID(N'dbo.AttendanceWeekPatterns', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.AttendanceWeekPatterns;
END
GO
