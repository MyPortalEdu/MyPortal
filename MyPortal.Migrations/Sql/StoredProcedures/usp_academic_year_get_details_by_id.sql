SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Full read of one academic year as 4 result sets:
--   1) header (AY scalars), 2) terms, 3) holidays joined to the backing DiaryEvent
--   for name/dates/type, 4) attendance periods. Returns nothing (no header) when
--   the AY doesn't exist -- caller maps that to 404.
CREATE OR ALTER PROCEDURE [dbo].[usp_academic_year_get_details_by_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.AcademicYears WHERE Id = @academicYearId)
    BEGIN
        RETURN;
    END

    -- 1) Header.
    SELECT
        Id                   = AY.Id,
        [Name]               = AY.[Name],
        IsLocked             = AY.IsLocked,
        TimetableCycleLength = AY.TimetableCycleLength,
        SchoolWeekLength     = AY.SchoolWeekLength
    FROM dbo.AcademicYears AS AY
    WHERE AY.Id = @academicYearId;

    -- 2) Terms.
    SELECT
        Id        = AT.Id,
        [Name]    = AT.[Name],
        StartDate = AT.StartDate,
        EndDate   = AT.EndDate
    FROM dbo.AcademicTerms AS AT
    WHERE AT.AcademicYearId = @academicYearId
    ORDER BY AT.StartDate;

    -- 3) Holidays. SchoolHolidays.EventId points at a DiaryEvent that owns the
    --    name / date range / type for the holiday; join to surface those.
    --    The service maps DiaryEventTypeId back to the SchoolHolidayType enum.
    SELECT
        Id          = SH.Id,
        [Name]      = DE.Subject,
        EventTypeId = DE.EventTypeId,
        StartDate   = DE.StartTime,
        EndDate     = DE.EndTime
    FROM dbo.SchoolHolidays AS SH
    JOIN dbo.DiaryEvents    AS DE ON DE.Id = SH.EventId
    WHERE SH.AcademicYearId = @academicYearId
    ORDER BY DE.StartTime;

    -- 4) Periods.
    SELECT
        Id            = AP.Id,
        [Name]        = AP.[Name],
        CycleDayIndex = AP.CycleDayIndex,
        StartTime     = AP.StartTime,
        EndTime       = AP.EndTime,
        IsAmReg       = AP.IsAmReg,
        IsPmReg       = AP.IsPmReg,
        IsLesson      = AP.IsLesson
    FROM dbo.AttendancePeriods AS AP
    WHERE AP.AcademicYearId = @academicYearId
    ORDER BY AP.CycleDayIndex, AP.StartTime;
END;
GO
