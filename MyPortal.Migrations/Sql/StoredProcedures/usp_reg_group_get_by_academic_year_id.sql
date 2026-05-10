SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_reg_group_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

-- RegGroups are year-scoped indirectly through StudentGroups.AcademicYearId.
SELECT
    RG.Id,
    RG.StudentGroupId,
    RG.YearGroupId,
    RG.RoomId,
    RG.CreatedById,
    RG.CreatedByIpAddress,
    RG.CreatedAt,
    RG.LastModifiedById,
    RG.LastModifiedByIpAddress,
    RG.LastModifiedAt,
    RG.Version
FROM
    dbo.RegGroups RG
JOIN
    dbo.StudentGroups SG ON SG.Id = RG.StudentGroupId
WHERE
    SG.AcademicYearId = @academicYearId;
END;
