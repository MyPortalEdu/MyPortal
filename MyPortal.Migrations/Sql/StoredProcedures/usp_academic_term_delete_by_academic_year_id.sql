SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_academic_term_delete_by_academic_year_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.AcademicTerms
    WHERE AcademicYearId = @academicYearId;
END;
