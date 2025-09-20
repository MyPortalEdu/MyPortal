SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER VIEW dbo.vw_attendance_period_instances
AS
WITH Base AS
(
    SELECT
        AP.Id                               AS PeriodId,
        AW.Id                               AS AttendanceWeekId,
        AP.WeekPatternId,
        AP.Weekday,
        AP.[Name],
        AP.StartTime,
        AP.EndTime,
        AP.IsAmReg,
        AP.IsPmReg,
        AW.Beginning,
        AW.IsNonTimetable,
        AT.StartDate       AS TermStartDate,
        AT.EndDate         AS TermEndDate
    FROM dbo.AttendancePeriods        AS AP
    LEFT JOIN dbo.AttendanceWeekPatterns AS AWP ON AWP.Id = AP.WeekPatternId
    LEFT JOIN dbo.AttendanceWeeks        AS AW  ON AW.WeekPatternId = AWP.Id
    LEFT JOIN dbo.AcademicTerms          AS AT  ON AT.Id = AW.AcademicTermId
),
Times AS
(
    SELECT
        b.*,
        -- Shift week beginning to the specific weekday (0=Sun → +6, else weekday-1)
        CAST(DATEADD(DAY, CASE WHEN b.Weekday = 0 THEN 6 ELSE b.Weekday - 1 END,
                     CAST(b.Beginning AS date)) AS datetime2(7)) AS BaseDate,
        -- Seconds since midnight for start/end
        DATEDIFF(SECOND, CAST('00:00:00' AS time(0)), CAST(b.StartTime AS time(0))) AS StartSec,
        DATEDIFF(SECOND, CAST('00:00:00' AS time(0)), CAST(b.EndTime   AS time(0))) AS EndSec
    FROM Base AS b
),
Inst AS
(
    SELECT
        t.*,
        DATEADD(SECOND, t.StartSec, t.BaseDate) AS ActualStartTime,
        DATEADD(SECOND, t.EndSec,   t.BaseDate) AS ActualEndTime
    FROM Times AS t
)
SELECT DISTINCT
    i.ActualStartTime,
    i.ActualEndTime,
    i.PeriodId,
    i.AttendanceWeekId,
    i.WeekPatternId,
    i.Weekday,
    i.[Name],
    i.StartTime,
    i.EndTime,
    i.IsAmReg,
    i.IsPmReg
FROM Inst AS i
    OUTER APPLY dbo.fn_diary_event_get_overlapping(
               i.ActualStartTime, i.ActualEndTime,
               CAST('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F28' AS uniqueidentifier)) AS OE
WHERE
-- keep periods within the term window [StartDate, EndDate + 1 day)
    i.ActualStartTime >= i.TermStartDate
  AND i.ActualEndTime < DATEADD(DAY, 1, i.TermEndDate)
  AND (
    (OE.Id IS NULL AND i.IsNonTimetable = 0)  -- exclude periods overlapped by events unless week is “non-timetable”
   OR i.IsAmReg = 1
   OR i.IsPmReg = 1
    );
GO
