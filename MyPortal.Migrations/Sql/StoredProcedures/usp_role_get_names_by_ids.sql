SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_role_get_names_by_ids]
    @roleIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    R.[Name]
FROM [dbo].[Roles] [R]
    INNER JOIN @roleIds RI ON RI.[Value] = R.[Id]
END;