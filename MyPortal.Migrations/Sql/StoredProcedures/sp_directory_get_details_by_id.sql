SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_directory_get_details_by_id] 
    @directoryId UNIQUEIDENTIFIER,
    @isStaff BIT
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    D.[Id],
    D.[ParentId],
    D.[Name],
    D.[IsPrivate],
    D.[UploadPolicy]
FROM [Directories] [D]
WHERE D.[Id] = @directoryId
  AND (@isStaff = 1 OR D.[IsPrivate] = 0)

END;
