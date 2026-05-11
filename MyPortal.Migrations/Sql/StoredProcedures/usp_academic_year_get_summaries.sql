SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Lists every academic year with its derived term-range bounds. Ordered most-recent
-- first so the UI's default sort is "what just happened / what's coming".
CREATE OR ALTER PROCEDURE [dbo].[usp_academic_year_get_summaries]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AY.Id,
        AY.[Name],
        AY.IsLocked,
        AY.TimetableCycleLength,
        AY.SchoolWeekLength,
        StartDate = (SELECT MIN(StartDate) FROM dbo.AcademicTerms AT WHERE AT.AcademicYearId = AY.Id),
        EndDate   = (SELECT MAX(EndDate)   FROM dbo.AcademicTerms AT WHERE AT.AcademicYearId = AY.Id)
    FROM dbo.AcademicYears AS AY
    ORDER BY StartDate DESC, AY.[Name] DESC;
END;
GO
