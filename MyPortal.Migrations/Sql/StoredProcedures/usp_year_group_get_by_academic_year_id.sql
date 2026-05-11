SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_year_group_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

-- YearGroups are year-scoped indirectly through StudentGroups.AcademicYearId.
SELECT
    YG.Id,
    YG.StudentGroupId,
    YG.CurriculumYearGroupId,
    YG.CreatedById,
    YG.CreatedByIpAddress,
    YG.CreatedAt,
    YG.LastModifiedById,
    YG.LastModifiedByIpAddress,
    YG.LastModifiedAt,
    YG.Version
FROM
    dbo.YearGroups YG
JOIN
    dbo.StudentGroups SG ON SG.Id = YG.StudentGroupId
WHERE
    SG.AcademicYearId = @academicYearId;
END;
