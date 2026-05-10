SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_student_group_supervisor_get_by_student_group_id]
    @studentGroupId UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;

SELECT
    SGS.Id,
    SGS.StudentGroupId,
    SGS.SupervisorId,
    SGS.Title
FROM
    dbo.StudentGroupSupervisors SGS
WHERE
    SGS.StudentGroupId = @studentGroupId;
END;
