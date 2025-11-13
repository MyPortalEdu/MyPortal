SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_directory_get_details_by_parent_id]
    @directoryId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    D.[Id],
    D.[ParentId],
    D.[Name],
    D.[IsPrivate]
FROM [Directories] [D]
WHERE D.[ParentId] = @directoryId;

END;
