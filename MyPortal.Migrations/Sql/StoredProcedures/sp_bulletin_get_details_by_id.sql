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
AND @canView = 1
AND (@isStaff = 1 OR B.IsPrivate = 0)
AND (
        @canApprove = 1
        OR (
                (@isStaff = 1 AND @canEdit = 1 AND B.CreatedById = @currentUserId)
                OR (
                        B.IsApproved = 1
                        AND (B.ExpiresAt IS NULL OR B.ExpiresAt > @nowUtc)                    
    )
    )
    )
END;