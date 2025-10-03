SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_role_get_names_by_ids]
    @roleIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    R.[Name]
FROM [Roles] [R]
    INNER JOIN @roleIds RI ON RI.[Value] = R.[Id]
END;