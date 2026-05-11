SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_attendance_period_delete_by_academic_year_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.AttendancePeriods
    WHERE AcademicYearId = @academicYearId;
END;
