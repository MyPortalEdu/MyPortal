SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Contact profile header — identity row shown at the top of the page. A contact has no lifecycle,
-- admission number or registration, so this is identity + photo only. DisplayName uses
-- fn_person_get_name format 1 (Title First [Middle] Last) on the legal name; PreferredName is the
-- preferred first name (or null).
CREATE OR ALTER PROCEDURE [dbo].[usp_contact_get_header_by_id]
    @contactId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [C].[Id],
    [C].[PersonId],
    [N].[Name] AS [DisplayName],
    NULLIF([P].[PreferredFirstName], '') AS [PreferredName],
    [P].[PhotoId]
FROM [dbo].[Contacts] [C]
    INNER JOIN [dbo].[People] [P] ON [C].[PersonId] = [P].[Id]
    CROSS APPLY [dbo].[fn_person_get_name]([C].[PersonId], 1, 0, 1) [N]
WHERE [C].[Id] = @contactId;
END
