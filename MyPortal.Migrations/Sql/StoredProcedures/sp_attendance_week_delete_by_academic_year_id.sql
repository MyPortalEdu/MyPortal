SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[sp_attendance_week_delete_by_academic_year_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE AW
      FROM dbo.AttendanceWeeks AW
      JOIN dbo.AcademicTerms   AT ON AT.Id = AW.AcademicTermId
     WHERE AT.AcademicYearId = @academicYearId;
END;
