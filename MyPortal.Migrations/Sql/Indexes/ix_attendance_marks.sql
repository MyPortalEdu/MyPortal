-- Unique attendance mark per student, week and period
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AttendanceMarks_Student_Week_Period'
      AND object_id = OBJECT_ID('dbo.AttendanceMarks')
)
BEGIN
CREATE UNIQUE INDEX UX_AttendanceMarks_Student_Week_Period
    ON dbo.AttendanceMarks(StudentId, AttendanceWeekId, AttendancePeriodId);
END