SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Student profile header — identity row shown at the top of the page, plus the admission/leaving
-- dates the service needs to derive status. DisplayName uses fn_person_get_name format 1
-- (Title First [Middle] Last) on the legal name; PreferredName is the preferred first name (or null).
-- The service derives the lifecycle badge (Active / Future / Leaver / None) from the dates, with
-- IsDeleted (Archived) taking precedence.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_get_header_by_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [S].[Id],
    [S].[PersonId],
    [S].[AdmissionNumber],
    [S].[IsDeleted],
    [N].[Name] AS [DisplayName],
    NULLIF([P].[PreferredFirstName], '') AS [PreferredName],
    [P].[PhotoId],
    [S].[DateStarting],
    [S].[DateLeaving]
FROM [dbo].[Students] [S]
    INNER JOIN [dbo].[People] [P] ON [S].[PersonId] = [P].[Id]
    CROSS APPLY [dbo].[fn_person_get_name]([S].[PersonId], 1, 0, 1) [N]
WHERE [S].[Id] = @studentId;
END
