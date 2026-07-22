SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's dated SEN status history. The open row (null EndDate) is the current status.
CREATE OR ALTER PROCEDURE [dbo].[usp_sen_status_history_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [SenStatusId], [StartDate], [EndDate]
    FROM [dbo].[SenStatusHistories]
    WHERE [StudentId] = @studentId;
END;
