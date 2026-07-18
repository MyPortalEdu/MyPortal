SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_list_by_academic_year]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.Timetables
     WHERE AcademicYearId = @academicYearId
     ORDER BY CreatedAt DESC;
END;
