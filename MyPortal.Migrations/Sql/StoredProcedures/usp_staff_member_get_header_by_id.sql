SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Staff profile header — identity row shown at the top of the page, plus the
-- bits the service needs to derive status. DisplayName uses fn_person_get_name
-- format 1 (Title First [Middle] Last) on the legal name; PreferredName is the
-- preferred first name (or null).
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_header_by_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [SM].[Id],
    [SM].[PersonId],
    [SM].[Code],
    [SM].[IsDeleted],
    [N].[Name] AS [DisplayName],
    NULLIF([P].[PreferredFirstName], '') AS [PreferredName],
    [P].[PhotoId]
FROM [dbo].[StaffMembers] [SM]
    INNER JOIN [dbo].[People] [P] ON [SM].[PersonId] = [P].[Id]
    CROSS APPLY [dbo].[fn_person_get_name]([SM].[PersonId], 1, 0, 1) [N]
WHERE [SM].[Id] = @staffMemberId;
END
