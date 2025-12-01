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
SELECT 
    dir.Id,
    dir.ParentId,
    dir.Name,
    dir.IsPrivate
FROM RecursiveDirectories dir