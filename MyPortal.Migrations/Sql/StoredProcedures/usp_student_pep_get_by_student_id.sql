SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's Personal Education Plans (dated; contributors are loaded separately by pep id).
CREATE OR ALTER PROCEDURE [dbo].[usp_student_pep_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [StartDate], [EndDate], [Comment]
    FROM [dbo].[StudentPeps]
    WHERE [StudentId] = @studentId;
END;
