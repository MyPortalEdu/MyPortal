SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's looked-after (in-care) episodes; the current episode is the open one (EndDate null).
CREATE OR ALTER PROCEDURE [dbo].[usp_student_care_episode_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [CaringAuthorityId], [LivingArrangementId], [StartDate], [EndDate], [Comment]
    FROM [dbo].[StudentCareEpisodes]
    WHERE [StudentId] = @studentId;
END;
