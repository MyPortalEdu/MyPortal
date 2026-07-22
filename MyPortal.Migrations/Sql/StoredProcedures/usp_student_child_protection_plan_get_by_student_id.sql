SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's child protection plans (dated safeguarding records).
CREATE OR ALTER PROCEDURE [dbo].[usp_student_child_protection_plan_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [LocalAuthorityId], [StartDate], [EndDate], [Comment]
    FROM [dbo].[StudentChildProtectionPlans]
    WHERE [StudentId] = @studentId;
END;
