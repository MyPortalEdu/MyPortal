SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_user_get_details_by_id] 
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
CROSS APPLY [dbo].[fn_person_get_name](U.PersonId, 3, 0, 0) AS P
WHERE [U].[Id] = @userId
END;