SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_house_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

-- Houses are year-scoped indirectly through StudentGroups.AcademicYearId.
SELECT
    H.Id,
    H.StudentGroupId,
    H.ColourCode
FROM
    dbo.Houses H
JOIN
    dbo.StudentGroups SG ON SG.Id = H.StudentGroupId
WHERE
    SG.AcademicYearId = @academicYearId;
END;
