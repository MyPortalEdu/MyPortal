-- ============================================================================
-- Switch attendance week patterns to a cycle-day model.
--
-- Before: an AttendanceWeekPattern was a single calendar week. Fortnightly
-- schools needed two patterns alternating, and AttendanceWeek pointed at
-- whichever was active that week.
--
-- After: a pattern is a CycleLength-day cycle (5 = weekly, 10 = fortnightly,
-- 6 = six-day rotation, …). AttendancePeriod uses CycleDayIndex (0..N-1)
-- instead of Weekday. AttendanceWeek's CycleOffset says which cycle-day Monday
-- of that calendar week lands on. With one pattern per academic year the
-- WeekPatternId column on AttendanceWeek goes away — the pattern is reached
-- via Term → Year.
-- ============================================================================

-- vw_attendance_period_instances reads the columns we're about to rename / drop.
-- The view-deployment step recreates it after this update runs.
IF OBJECT_ID(N'dbo.vw_session_period_metadata', N'V') IS NOT NULL
    DROP VIEW dbo.vw_session_period_metadata;
GO
IF OBJECT_ID(N'dbo.vw_attendance_period_instances', N'V') IS NOT NULL
    DROP VIEW dbo.vw_attendance_period_instances;
GO

-- ─── AttendanceWeekPatterns: + AcademicYearId, + CycleLength ─────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns') AND name = N'AcademicYearId'
)
BEGIN
    ALTER TABLE dbo.AttendanceWeekPatterns
        ADD AcademicYearId UNIQUEIDENTIFIER NULL;
END
GO

DECLARE @defaultAcademicYearId UNIQUEIDENTIFIER;
SELECT TOP 1 @defaultAcademicYearId = Id FROM dbo.AcademicYears ORDER BY Name;

IF @defaultAcademicYearId IS NULL
   AND EXISTS (SELECT 1 FROM dbo.AttendanceWeekPatterns WHERE AcademicYearId IS NULL)
BEGIN
    ;THROW 50018, 'Update 0018 cannot run: dbo.AttendanceWeekPatterns has existing rows but dbo.AcademicYears is empty. Seed at least one academic year before applying.', 1;
END

UPDATE dbo.AttendanceWeekPatterns
SET AcademicYearId = @defaultAcademicYearId
WHERE AcademicYearId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns')
      AND name = N'AcademicYearId' AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.AttendanceWeekPatterns
        ALTER COLUMN AcademicYearId UNIQUEIDENTIFIER NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_AttendanceWeekPatterns_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns')
)
BEGIN
    ALTER TABLE dbo.AttendanceWeekPatterns
        ADD CONSTRAINT FK_AttendanceWeekPatterns_AcademicYears
            FOREIGN KEY (AcademicYearId) REFERENCES dbo.AcademicYears(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_AttendanceWeekPatterns_AcademicYearId'
      AND object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns')
)
BEGIN
    CREATE INDEX IX_AttendanceWeekPatterns_AcademicYearId
        ON dbo.AttendanceWeekPatterns(AcademicYearId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendanceWeekPatterns') AND name = N'CycleLength'
)
BEGIN
    -- Default 5 = weekly. Fortnightly schools update to 10 during AY setup.
    ALTER TABLE dbo.AttendanceWeekPatterns
        ADD CycleLength INT NOT NULL CONSTRAINT DF_AttendanceWeekPatterns_CycleLength DEFAULT (5);
END
GO

-- ─── AttendanceWeeks: + CycleOffset, − WeekPatternId ─────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendanceWeeks') AND name = N'CycleOffset'
)
BEGIN
    ALTER TABLE dbo.AttendanceWeeks
        ADD CycleOffset INT NOT NULL CONSTRAINT DF_AttendanceWeeks_CycleOffset DEFAULT (0);
END
GO

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_AttendanceWeeks_WeekPatternId_AttendanceWeekPatterns'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendanceWeeks')
)
BEGIN
    ALTER TABLE dbo.AttendanceWeeks
        DROP CONSTRAINT FK_AttendanceWeeks_WeekPatternId_AttendanceWeekPatterns;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_AttendanceWeeks_WeekPatternId'
      AND object_id = OBJECT_ID(N'dbo.AttendanceWeeks')
)
BEGIN
    DROP INDEX IX_AttendanceWeeks_WeekPatternId ON dbo.AttendanceWeeks;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendanceWeeks') AND name = N'WeekPatternId'
)
BEGIN
    ALTER TABLE dbo.AttendanceWeeks DROP COLUMN WeekPatternId;
END
GO

-- ─── AttendancePeriods: Weekday → CycleDayIndex ──────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'CycleDayIndex'
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods ADD CycleDayIndex INT NULL;
END
GO

-- Backfill from the old Weekday column. Existing data may be stored either as the DayOfWeek
-- enum's int value or its string name; handle both. The mapping is .NET DayOfWeek's natural
-- ordering (Sun=0..Sat=6).
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'Weekday'
)
BEGIN
    UPDATE dbo.AttendancePeriods
    SET CycleDayIndex =
        CASE
            WHEN TRY_CAST(Weekday AS INT) IS NOT NULL THEN TRY_CAST(Weekday AS INT)
            WHEN Weekday = N'Sunday'    THEN 0
            WHEN Weekday = N'Monday'    THEN 1
            WHEN Weekday = N'Tuesday'   THEN 2
            WHEN Weekday = N'Wednesday' THEN 3
            WHEN Weekday = N'Thursday'  THEN 4
            WHEN Weekday = N'Friday'    THEN 5
            WHEN Weekday = N'Saturday'  THEN 6
            ELSE 0
        END
    WHERE CycleDayIndex IS NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods')
      AND name = N'CycleDayIndex' AND is_nullable = 1
)
BEGIN
    UPDATE dbo.AttendancePeriods SET CycleDayIndex = 0 WHERE CycleDayIndex IS NULL;
    ALTER TABLE dbo.AttendancePeriods ALTER COLUMN CycleDayIndex INT NOT NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'Weekday'
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods DROP COLUMN Weekday;
END
GO
