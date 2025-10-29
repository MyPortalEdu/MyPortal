SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_bulletin_get_details_by_id] 
    @bulletinId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    B.Id,
    B.DirectoryId,
    B.ExpiresAt,
    B.Title,
    B.Detail,
    B.IsPrivate,
    B.IsApproved,
    B.CreatedById,
    COALESCE(CBUN.Name, CBU.UserName) AS CreatedByName,
    B.CreatedByIpAddress,
    B.CreatedAt,
    B.LastModifiedById,
    COALESCE(MBUN.Name, MBU.UserName) AS LastModifiedByName,
    B.LastModifiedByIpAddress,
    B.LastModifiedAt
FROM dbo.Bulletins B
         INNER JOIN dbo.Users CBU ON CBU.Id = B.CreatedById
         INNER JOIN dbo.Users MBU ON MBU.Id = B.LastModifiedById
    OUTER APPLY dbo.fn_person_get_name (CBU.PersonId, 2, 0, 0) AS CBUN
OUTER APPLY dbo.fn_person_get_name (MBU.PersonId, 2, 0, 0) AS MBUN
WHERE B.Id = @bulletinId
END;