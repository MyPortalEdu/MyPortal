SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_role_get_details_by_id] 
    @roleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    R.[Id],
    R.[Description],
    R.[IsSystem],
    R.[Name]        
FROM Roles [R]
WHERE R.[Id] = @roleId;

END;