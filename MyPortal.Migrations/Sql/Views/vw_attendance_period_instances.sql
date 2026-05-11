SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER VIEW dbo.vw_attendance_period_instances
AS
-- An AttendancePeriod belongs to an AcademicYear's TimetableCycleLength-day cycle
-- (5 = weekly, 10 = fortnightly, …). Each AttendanceWeek records which cycle-day
-- Monday of that calendar week corresponds to (CycleOffset). For a period to
-- render in a given calendar week, its (CycleDayIndex - CycleOffset) mod
-- TimetableCycleLength must fall within the school's operating week — currently
-- hardcoded to Mon-Fri.
WITH Base AS
(
    SELECT
        AP.Id                                AS PeriodId,
        AW.Id                                AS AttendanceWeekId,
        AP.AcademicYearId,
        AP.CycleDayIndex,
        AW.CycleOffset,
        AY.TimetableCycleLength,
        AP.[Name],
        AP.StartTime,
        AP.EndTime,
        AP.IsAmReg,
        AP.IsPmReg,
        AW.Beginning,
        AW.IsNonTimetable,
        AT.StartDate                         AS TermStartDate,
        AT.EndDate                           AS TermEndDate,
        ((AP.CycleDayIndex - AW.CycleOffset + AY.TimetableCycleLength) % AY.TimetableCycleLength)
                                             AS RelativeDay
    FROM dbo.AttendancePeriods         AS AP
    JOIN dbo.AcademicYears             AS AY ON AY.Id = AP.AcademicYearId
    JOIN dbo.AcademicTerms             AS AT ON AT.AcademicYearId = AY.Id
    JOIN dbo.AttendanceWeeks           AS AW ON AW.AcademicTermId = AT.Id
),
Times AS
(
    SELECT
        b.*,
        -- Beginning is the Monday of the calendar week; RelativeDay (0..4) gives the
        -- Mon-Fri offset to land on the period's actual date.
        CAST(DATEADD(DAY, b.RelativeDay, CAST(b.Beginning AS date)) AS datetime2(7)) AS BaseDate,
        DATEDIFF(SECOND, CAST('00:00:00' AS time(0)), CAST(b.StartTime AS time(0))) AS StartSec,
        DATEDIFF(SECOND, CAST('00:00:00' AS time(0)), CAST(b.EndTime   AS time(0))) AS EndSec
    FROM Base AS b
    -- Mon-Fri school week. A period whose RelativeDay >= 5 belongs to a different
    -- calendar week of the cycle (e.g. a Week B period viewed from a Week A week).
    WHERE b.RelativeDay < 5
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
    i.AcademicYearId,
    i.CycleDayIndex,
    i.[Name],
    i.StartTime,
    i.EndTime,
    i.IsAmReg,
    i.IsPmReg
FROM Inst AS i
WHERE
    i.ActualStartTime >= i.TermStartDate
  AND i.ActualEndTime < DATEADD(DAY, 1, i.TermEndDate)
  AND (
        -- Skip periods inside a non-timetabled week (e.g. INSET) or overlapped by a
        -- non-teaching diary event. AM/PM reg always shows so registers can still be
        -- taken on those days. Kind values map to MyPortal.Common.Enums.DiaryEventKind:
        -- 7 = SchoolHoliday, 8 = PublicHoliday, 9 = TeacherTraining.
        (
            i.IsNonTimetable = 0
            AND NOT EXISTS (
                SELECT 1
                FROM dbo.fn_diary_event_get_overlapping(i.ActualStartTime, i.ActualEndTime) AS OE
                JOIN dbo.DiaryEventTypes AS DET ON DET.Id = OE.EventTypeId
                WHERE DET.Kind IN (7, 8, 9)
            )
        )
        OR i.IsAmReg = 1
        OR i.IsPmReg = 1
    );
GO
