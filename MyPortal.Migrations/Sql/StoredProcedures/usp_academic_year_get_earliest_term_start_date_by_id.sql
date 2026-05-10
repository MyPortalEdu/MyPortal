SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_academic_year_get_earliest_term_start_date_by_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MIN(StartDate)
    FROM dbo.AcademicTerms
    WHERE AcademicYearId = @academicYearId;
END;
