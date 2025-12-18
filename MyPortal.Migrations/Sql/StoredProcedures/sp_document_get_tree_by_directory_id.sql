SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_document_get_tree_by_directory_id] 
    @directoryId UNIQUEIDENTIFIER,
    @includeDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

WITH RecursiveDirectories AS
         (
             -- Start with the root directory
             SELECT r.Id, r.ParentId, r.Name, r.IsPrivate
             FROM dbo.Directories r
             WHERE Id = @directoryId

             UNION ALL

             -- Recursively get children
             SELECT d.Id, d.ParentId, d.Name, d.IsPrivate
             FROM dbo.Directories d
                      INNER JOIN RecursiveDirectories rd ON d.ParentId = rd.Id
         )
SELECT [D].Id,
    [D].CreatedById,
    COALESCE(UCN.[Name], UC.[UserName]) AS CreatedByName,
    [D].CreatedByIpAddress,
    [D].CreatedAt,
    [D].LastModifiedById,
    COALESCE(UMN.[Name], UM.[UserName]) AS LastModifiedByName,
    [D].LastModifiedByIpAddress,
    [D].LastModifiedAt,
    [D].TypeId,
    [DT].Description AS TypeDescription,
    [D].DirectoryId,
    [D].StorageKey,
    [D].FileName,
    [D].ContentType,
    [D].SizeBytes,
    [D].Hash,
    [D].Title,
    [D].Description,
    [D].IsPrivate,
    [D].IsDeleted
FROM dbo.Documents [D]
INNER JOIN RecursiveDirectories dir ON [D].DirectoryId = dir.Id
INNER JOIN dbo.[DocumentTypes] [DT] ON [D].[TypeId] = [DT].[Id]
INNER JOIN dbo.[Users] [UC] ON [D].[CreatedById] = [UC].[Id]
INNER JOIN dbo.[Users] [UM] ON [D].[LastModifiedById] = [UM].[Id]
OUTER APPLY dbo.fn_person_get_name (UC.[PersonId], 3, 0, 1) AS UCN
OUTER APPLY dbo.fn_person_get_name (UM.[PersonId], 3, 0, 1) AS UMN
WHERE ([D].IsDeleted = 0 OR @includeDeleted = 1)
ORDER BY dir.Name, [D].Title

END;