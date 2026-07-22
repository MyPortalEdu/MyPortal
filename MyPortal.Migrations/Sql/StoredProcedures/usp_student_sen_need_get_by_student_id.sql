SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's SEN needs (dated, ranked; Rank 1 = primary).
CREATE OR ALTER PROCEDURE [dbo].[usp_student_sen_need_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [SenTypeId], [Description], [StartDate], [EndDate], [Rank]
    FROM [dbo].[StudentSenNeeds]
    WHERE [StudentId] = @studentId;
END;
