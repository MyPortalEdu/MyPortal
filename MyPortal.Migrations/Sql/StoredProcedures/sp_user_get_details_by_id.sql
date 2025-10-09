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
    [P].[Name] AS [PersonFullName],
    [U].[UserName] AS [Username],
    [U].[NormalizedUserName] AS [NormalizedUsername],
    [U].[Email],
    [U].[NormalizedEmail],
    [U].[EmailConfirmed],
    [U].[SecurityStamp],
    [U].[ConcurrencyStamp],
    [U].[PhoneNumber],
    [U].[PhoneNumberConfirmed],
    [U].[TwoFactorEnabled],
    [U].[LockoutEnd],
    [U].[LockoutEnabled],
    [U].[AccessFailedCount]
FROM [dbo].[Users] [U]
CROSS APPLY [dbo].[fn_person_get_name](U.PersonId, 3, 1, 0) AS P
WHERE [U].[Id] = @userId
END;