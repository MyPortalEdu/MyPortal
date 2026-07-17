SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_user_get_details_by_id] 
    @userId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [U].[Id],
    [U].[CreatedAt],
    [U].[PersonId],
    [U].[UserType],
    [U].[IsEnabled],
    [U].[IsSystem],
    [P].[Name] AS [PersonFullName],
    [U].[UserName] AS [Username],    
    [U].[Email],            
    [U].[PhoneNumber],
    [U].[TwoFactorEnabled],    
    [U].[LockoutEnabled]    
FROM [dbo].[Users] [U]
-- OUTER APPLY (not CROSS) so users with no linked Person — e.g. the admin / _system accounts with a
-- NULL PersonId — aren't dropped. Matches GetUserSummaries.sql, which is why they show in the list.
OUTER APPLY [dbo].[fn_person_get_name](U.PersonId, 3, 0, 0) AS P
WHERE [U].[Id] = @userId
END;