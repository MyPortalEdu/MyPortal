SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_directory_is_in_subtree]
    @rootDirectoryId UNIQUEIDENTIFIER,
    @candidateDirectoryId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF (@rootDirectoryId = @candidateDirectoryId)
BEGIN
SELECT CAST(1 AS BIT) AS IsInSubtree;
RETURN;
END

    ;WITH UpTree AS
              (
                  SELECT d.Id, d.ParentId
                  FROM dbo.Directories d
                  WHERE d.Id = @candidateDirectoryId
                    AND d.IsDeleted = 0

                  UNION ALL

                  SELECT parent.Id, parent.ParentId
                  FROM dbo.Directories parent
                           INNER JOIN UpTree c ON c.ParentId = parent.Id
                  WHERE parent.IsDeleted = 0
              )
     SELECT
         CAST(CASE WHEN EXISTS (
             SELECT 1
             FROM UpTree
             WHERE Id = @rootDirectoryId
         ) THEN 1 ELSE 0 END AS BIT) AS IsInSubtree
         OPTION (MAXRECURSION 32767);
END;