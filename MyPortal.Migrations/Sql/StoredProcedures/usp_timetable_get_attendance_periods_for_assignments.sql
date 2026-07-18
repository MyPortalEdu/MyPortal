SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Pull every period in the timetable's AcademicYear — materialisation needs the full
-- set in order to walk consecutive periods within a day.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_get_attendance_periods_for_assignments]
    @timetableId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AP.*
      FROM dbo.AttendancePeriods AP
      JOIN dbo.Timetables T ON T.AcademicYearId = AP.AcademicYearId
     WHERE T.Id = @timetableId;
END;
