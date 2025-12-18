SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_document_get_details_by_id] 
    @documentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    D.[Id],
    D.[TypeId],
    DT.[Description] AS TypeDescription,
    COALESCE(UCN.[Name], UC.[UserName]) AS CreatedByName,
    D.[CreatedAt],
    COALESCE(UMN.[Name], UM.[UserName]) AS LastModifiedByName,
    D.[LastModifiedAt],
    D.[StorageKey],
    D.[FileName],
    D.[ContentType],
    D.[SizeBytes],
    D.[Hash],
    D.[Title],
    D.[Description],
    D.[IsPrivate],
    D.[IsDeleted]
FROM [Documents] [D]
INNER JOIN dbo.[DocumentTypes] [DT] ON [D].[TypeId] = [DT].[Id]
INNER JOIN dbo.[Users] [UC] ON [D].[CreatedById] = [UC].[Id]
INNER JOIN dbo.[Users] [UM] ON [D].[LastModifiedById] = [UM].[Id]
OUTER APPLY dbo.fn_person_get_name (UC.[PersonId], 3, 0, 1) AS UCN
OUTER APPLY dbo.fn_person_get_name (UM.[PersonId], 3, 0, 1) AS UMN
WHERE D.[Id] = @documentId;

END;