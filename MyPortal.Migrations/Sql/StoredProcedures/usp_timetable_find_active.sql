SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_find_active]
    @academicYearId UNIQUEIDENTIFIER,
    @activeStatus INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 * FROM dbo.Timetables
     WHERE AcademicYearId = @academicYearId AND Status = @activeStatus;
END;
