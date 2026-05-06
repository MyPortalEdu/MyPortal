-- ============================================================================
-- Add SchoolWeekLength to AcademicYears.
--
-- The cycle math (AttendanceWeek.CycleOffset progression) needs to know how
-- many school days are in a calendar week independently of how long the cycle
-- itself is — a 5-day Mon-Fri school can have a 5-day cycle (weekly) or a
-- 10-day cycle (Week A / Week B), and the offset increment per calendar week
-- equals the school week length, not the cycle length.
--
-- Existing rows are backfilled to 5 (Mon-Fri, the UK default). The DEFAULT
-- constraint is dropped after backfill so service code is forced to set the
-- value explicitly going forward — silently defaulting to 5 would mask a
-- caller mistake on a Mon-Sat school.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AcademicYears') AND name = N'SchoolWeekLength'
)
BEGIN
    ALTER TABLE dbo.AcademicYears
        ADD SchoolWeekLength INT NOT NULL
            CONSTRAINT DF_AcademicYears_SchoolWeekLength DEFAULT (5);
END
GO

-- Drop the DEFAULT so future inserts have to specify the value — the backfill
-- has already happened, the default is no longer load-bearing.
IF EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE name = N'DF_AcademicYears_SchoolWeekLength'
      AND parent_object_id = OBJECT_ID(N'dbo.AcademicYears')
)
BEGIN
    ALTER TABLE dbo.AcademicYears
        DROP CONSTRAINT DF_AcademicYears_SchoolWeekLength;
END
GO
