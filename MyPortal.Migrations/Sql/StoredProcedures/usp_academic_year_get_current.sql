SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- "Current" academic year per UK semantics: the AY with the latest MIN(term.StartDate)
-- that is on or before today. This means during summer break (between the previous
-- AY's last term ending and the new one starting) the previous AY is still surfaced
-- as current, until the new AY's first term begins. Returns one row or nothing.
CREATE OR ALTER PROCEDURE [dbo].[usp_academic_year_get_current]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT TOP 1
        AY.Id,
        AY.[Name],
        AY.IsLocked,
        AY.TimetableCycleLength,
        AY.SchoolWeekLength,
        StartDate = Bounds.StartDate,
        EndDate   = Bounds.EndDate
    FROM dbo.AcademicYears AS AY
    CROSS APPLY (
        SELECT
            StartDate = MIN(AT.StartDate),
            EndDate   = MAX(AT.EndDate)
        FROM dbo.AcademicTerms AS AT
        WHERE AT.AcademicYearId = AY.Id
    ) AS Bounds
    WHERE Bounds.StartDate IS NOT NULL
      AND Bounds.StartDate <= @today
    ORDER BY Bounds.StartDate DESC;
END;
GO
