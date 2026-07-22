SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's SEN provisions (dated; open-ended when EndDate is null).
CREATE OR ALTER PROCEDURE [dbo].[usp_sen_provision_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [SenProvisionTypeId], [StartDate], [EndDate], [Frequency], [Cost], [Note]
    FROM [dbo].[SenProvisions]
    WHERE [StudentId] = @studentId;
END;
