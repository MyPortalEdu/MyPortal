SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_role_permission_get_by_role_id]
    @roleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    RP.[Id],
    RP.[RoleId],
    RP.[PermissionId]
FROM RolePermissions [RP]
WHERE RP.[RoleId] = @roleId;

END;
