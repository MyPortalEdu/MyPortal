SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_document_get_details_by_directory]
    @directoryId UNIQUEIDENTIFIER,
    @includeDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    D.[Id],
    D.[TypeId],
    DT.[Description] AS TypeDescription,
    CASE
        WHEN UC.PersonId IS NOT NULL
            THEN dbo.fn_person_get_name(UC.PersonId, 3, 0 ,1)
        ELSE UC.Username
    END AS CreatedByName,
    D.[CreatedAt],
    CASE
        WHEN UM.PersonId IS NOT NULL
            THEN dbo.fn_person_get_name(UM.PersonId, 3, 0 ,1)
        ELSE UM.Username
    END AS LastModifiedByName,
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
FROM [dbo].[Documents] [D]
INNER JOIN dbo.[DocumentTypes] [DT] ON [D].[TypeId] = [DT].[Id]
INNER JOIN dbo.[Users] [UC] ON [D].[CreatedById] = [UC].[Id]
INNER JOIN dbo.[Users] [UM] ON [D].[LastModifiedById] = [UM].[Id]
WHERE D.[DirectoryId] = @directoryId
AND (D.[IsDeleted] = 0 OR @includeDeleted = 1)

END;