-- ============================================================================
-- Add IsLesson to AttendancePeriods.
--
-- Some schools want a teaching period to double as a registration session
-- (e.g. the first lesson of Wednesday acts as that day's AM reg, taken by the
-- subject teacher rather than the form tutor). Previously every period was
-- implicitly a lesson unless flagged as AM/PM reg, which made the two cases
-- mutually exclusive. IsLesson decouples them: a period can be a lesson AND a
-- reg session, just a lesson, or just a reg session.
--
-- The materialisation views/SPs use IsLesson to route a period to the lesson
-- pipeline (Sessions/Classes) vs the reg-group pipeline (RegGroups), so the
-- two arms can no longer accidentally pick up each other's periods.
--
-- Backfill preserves prior behaviour: any period currently flagged as AM/PM
-- reg is treated as reg-only (IsLesson=0); everything else is a lesson. The
-- DEFAULT is dropped after backfill so callers must set the value explicitly.
--
-- The CHECK constraint enforces the invariant the wizard relies on: a period
-- has to participate in at least one pipeline, otherwise it would materialise
-- to nothing.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AttendancePeriods') AND name = N'IsLesson'
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods
        ADD IsLesson BIT NOT NULL
            CONSTRAINT DF_AttendancePeriods_IsLesson DEFAULT (1);
END
GO

UPDATE dbo.AttendancePeriods
SET IsLesson = CASE WHEN IsAmReg = 1 OR IsPmReg = 1 THEN 0 ELSE 1 END;
GO

IF EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE name = N'DF_AttendancePeriods_IsLesson'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods
        DROP CONSTRAINT DF_AttendancePeriods_IsLesson;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_AttendancePeriods_AtLeastOneRole'
      AND parent_object_id = OBJECT_ID(N'dbo.AttendancePeriods')
)
BEGIN
    ALTER TABLE dbo.AttendancePeriods
        ADD CONSTRAINT CK_AttendancePeriods_AtLeastOneRole
            CHECK (IsLesson = 1 OR IsAmReg = 1 OR IsPmReg = 1);
END
GO
