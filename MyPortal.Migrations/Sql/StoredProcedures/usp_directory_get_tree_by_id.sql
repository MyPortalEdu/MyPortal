SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_directory_get_tree_by_id] @directoryId UNIQUEIDENTIFIER, 
@isStaff BIT 
AS
BEGIN 
SET NOCOUNT ON;

WITH RecursiveDirectories AS (
    -- Root: only include it if staff, or it's not private
    SELECT
        Id,
        ParentId,
        Name,
        IsPrivate,
        UploadPolicy
    FROM dbo.Directories D
    WHERE
        Id = @directoryId
      AND IsDeleted = 0
      AND (@isStaff = 1 OR IsPrivate = 0)
    UNION ALL
    -- Children: only traverse from rows already included,
    -- and only include child if staff or child is not private
    SELECT
        d.Id,
        d.ParentId,
        d.Name,
        d.IsPrivate,
        d.UploadPolicy
    FROM
        dbo.Directories d
            INNER JOIN RecursiveDirectories rd ON d.ParentId = rd.Id
    WHERE
        d.IsDeleted = 0
      AND (@isStaff = 1 OR d.IsPrivate = 0)
)
SELECT
    dir.Id,
    dir.ParentId,
    dir.Name,
    dir.IsPrivate,
    dir.UploadPolicy
FROM
    RecursiveDirectories dir;
END;
