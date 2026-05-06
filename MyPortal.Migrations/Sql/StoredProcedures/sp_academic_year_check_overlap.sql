SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if the proposed [@rangeStart, @rangeEnd] interval overlaps with any
-- AcademicTerm belonging to a *different* academic year. @excludeAcademicYearId
-- lets the update path ignore the AY currently being edited; pass NULL on create.
-- Two ranges overlap when start1 <= end2 AND end1 >= start2.
CREATE OR ALTER PROCEDURE [dbo].[sp_academic_year_check_overlap]
    @excludeAcademicYearId UNIQUEIDENTIFIER = NULL,
    @rangeStart            DATETIME2(7),
    @rangeEnd              DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.AcademicTerms AT
        WHERE (@excludeAcademicYearId IS NULL OR AT.AcademicYearId <> @excludeAcademicYearId)
          AND AT.StartDate <= @rangeEnd
          AND AT.EndDate   >= @rangeStart
    ) THEN 1 ELSE 0 END AS bit);
END;
