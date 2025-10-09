SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_user_get_info_by_id] 
    @userId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [U].[Id],
    [U].[UserType],
    [U].[IsEnabled],
    COALESCE([P].[Name], [U].UserName) AS [DisplayName],
    [U].[UserName] AS [Username],
    [U].[Email]
FROM [dbo].[Users] [U]
OUTER APPLY [dbo].[fn_person_get_name](U.PersonId, 3, 1, 0) AS P
WHERE [U].[Id] = '00F1044E-33DF-4A21-8E54-6A481979FD1E'
END;