SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns the academic year a year group belongs to (via its backing StudentGroup), or
-- no row when the year group doesn't exist. Used to reject a reg group that points at a
-- year group from a different academic year.
CREATE OR ALTER PROCEDURE [dbo].[usp_year_group_get_academic_year_id]
    @yearGroupId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SG.AcademicYearId
    FROM dbo.YearGroups    AS YG
    JOIN dbo.StudentGroups AS SG ON SG.Id = YG.StudentGroupId
    WHERE YG.Id = @yearGroupId;
END;
GO
