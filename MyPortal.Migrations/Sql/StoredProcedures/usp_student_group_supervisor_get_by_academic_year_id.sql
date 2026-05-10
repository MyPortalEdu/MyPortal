SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_student_group_supervisor_get_by_academic_year_id] @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

-- StudentGroupSupervisors are year-scoped indirectly through StudentGroups.AcademicYearId.
SELECT
    SGS.Id,
    SGS.StudentGroupId,
    SGS.SupervisorId,
    SGS.Title
FROM
    dbo.StudentGroupSupervisors SGS
JOIN
    dbo.StudentGroups SG ON SG.Id = SGS.StudentGroupId
WHERE
    SG.AcademicYearId = @academicYearId;
END;
