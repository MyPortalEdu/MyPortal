WITH RecursiveDirectories AS
         (
             -- Start with the root directory
             SELECT Id, ParentId, Name, IsPrivate
             FROM dbo.Directories
             WHERE Id = @rootDirectoryId

             UNION ALL

             -- Recursively get children
             SELECT d.Id, d.ParentId, d.Name, d.IsPrivate
             FROM dbo.Directories d
                      INNER JOIN RecursiveDirectories rd ON d.ParentId = rd.Id
         )
SELECT [D].Id,
    [D].CreatedById,
    CASE
    WHEN UC.PersonId IS NOT NULL
    THEN dbo.[fn_person_get_name](UC.PersonId, 3, 0 ,1)
    ELSE UC.Username
END AS CreatedByName,
	   [D].CreatedByIpAddress,
	   [D].CreatedAt,
	   [D].LastModifiedById,
	   CASE
		   WHEN UM.PersonId IS NOT NULL
			   THEN dbo.[fn_person_get_name](UM.PersonId, 3, 0 ,1)
		   ELSE UM.Username
END AS LastModifiedByName,
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
ORDER BY dir.Name, [D].Title