SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_permission_get_by_user_id] 
    @userId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        [P].[Id],
        [P].[Name],
        [P].[FriendlyName],
        [P].[Area]
    FROM [dbo].[Permissions] [P]
    INNER JOIN [dbo].[RolePermissions] [RP] ON [RP].[PermissionId] = [P].[Id]
    INNER JOIN [dbo].[UserRoles] [UR] ON [UR].[RoleId] = [RP].[RoleId]
    WHERE [UR].[UserId] = @userId
    ORDER BY Name
END;