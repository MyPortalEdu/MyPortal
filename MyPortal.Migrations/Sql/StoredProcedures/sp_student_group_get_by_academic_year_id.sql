SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[sp_student_group_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

SELECT
    Id,
    Description,
    Active,
    Code,
    AcademicYearId,
    PromoteToGroupId,
    MainSupervisorId,
    MaxMembers,
    Notes,
    IsSystem,
    CreatedById,
    CreatedByIpAddress,
    CreatedAt,
    LastModifiedById,
    LastModifiedByIpAddress,
    LastModifiedAt,
    Version
FROM
    dbo.StudentGroups
WHERE
    AcademicYearId = @academicYearId
ORDER BY
    Code;
END;
