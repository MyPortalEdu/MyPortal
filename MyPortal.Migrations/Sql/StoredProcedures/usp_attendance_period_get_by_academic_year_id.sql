SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_attendance_period_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

SELECT
    Id,
    AcademicYearId,
    CycleDayIndex,
    Name,
    StartTime,
    EndTime,
    IsAmReg,
    IsPmReg
FROM
    dbo.AttendancePeriods
WHERE
    AcademicYearId = @academicYearId
ORDER BY
    CycleDayIndex,
    StartTime;
END;
