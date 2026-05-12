SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Full read of one bulletin as 3 result sets:
--   1) header (bulletin + category + audit names),
--   2) audiences (one row per target),
--   3) ack rollup (AcknowledgedCount, HasAcknowledged) — both NULL when the
--      bulletin does not require acknowledgement.
--
-- Returns nothing (no header) when the caller is not allowed to see this
-- bulletin — caller maps to 404. The visibility predicate matches
-- BulletinAccessPolicy + the audience model in one place:
--   • staff pinners see everything;
--   • staff creators see their own (even after expiry);
--   • otherwise the caller must be in the audience AND the bulletin must not
--     be expired.
CREATE OR ALTER PROCEDURE [dbo].[usp_bulletin_get_details_by_id]
    @bulletinId    UNIQUEIDENTIFIER,
    @currentUserId UNIQUEIDENTIFIER,
    @isStaff       BIT,
    @isPupil       BIT,
    @isParent      BIT,
    @canView       BIT,
    @canEdit       BIT,
    @canPin        BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @nowUtc DATETIME2(7) = SYSUTCDATETIME();

    -- Resolve the bulletin to a single row if (and only if) the caller may see it.
    -- We do this once into a temp table so the audience and ack result sets share
    -- the same gate without repeating the predicate.
    DECLARE @visible TABLE (Id UNIQUEIDENTIFIER);

    ;WITH PupilGroups AS (
        SELECT SGM.StudentGroupId
        FROM dbo.StudentGroupMemberships SGM
        JOIN dbo.Students                S   ON S.Id = SGM.StudentId
        JOIN dbo.Users                   U   ON U.PersonId = S.PersonId
        WHERE @isPupil = 1
          AND U.Id = @currentUserId
          AND SGM.StartDate <= @nowUtc
          AND (SGM.EndDate IS NULL OR SGM.EndDate > @nowUtc)
    ),
    ParentGroups AS (
        SELECT SGM.StudentGroupId
        FROM dbo.StudentGroupMemberships     SGM
        JOIN dbo.Students                    S   ON S.Id   = SGM.StudentId
        JOIN dbo.StudentContactRelationships SCR ON SCR.StudentId = S.Id
        JOIN dbo.Contacts                    C   ON C.Id   = SCR.ContactId
        JOIN dbo.Users                       U   ON U.PersonId = C.PersonId
        WHERE @isParent = 1
          AND U.Id = @currentUserId
          AND SGM.StartDate <= @nowUtc
          AND (SGM.EndDate IS NULL OR SGM.EndDate > @nowUtc)
    )
    INSERT INTO @visible (Id)
    SELECT B.Id
    FROM dbo.Bulletins B
    WHERE B.Id = @bulletinId
      AND (
            -- Staff pinners see everything (admin oversight).
            (@isStaff = 1 AND @canPin = 1)
         OR -- Staff creators always see their own bulletins, even expired.
            (@isStaff = 1 AND @canEdit = 1 AND B.CreatedById = @currentUserId)
         OR -- Otherwise: not expired AND audience matches the caller's role.
            (
                (B.ExpiresAt IS NULL OR B.ExpiresAt > @nowUtc)
                AND EXISTS (
                    SELECT 1
                    FROM dbo.BulletinAudiences BA
                    WHERE BA.BulletinId = B.Id
                      AND (
                            (@isStaff  = 1 AND BA.AudienceKind = 1) -- AllStaff
                         OR (@isPupil  = 1 AND BA.AudienceKind = 2) -- AllPupils
                         OR (@isParent = 1 AND BA.AudienceKind = 3) -- AllParents
                         OR (@isPupil  = 1 AND BA.AudienceKind = 4
                                            AND BA.StudentGroupId IN (SELECT StudentGroupId FROM PupilGroups))
                         OR (@isParent = 1 AND BA.AudienceKind = 4
                                            AND BA.StudentGroupId IN (SELECT StudentGroupId FROM ParentGroups))
                      )
                )
            )
          );

    IF NOT EXISTS (SELECT 1 FROM @visible) RETURN;

    -- 1) Header.
    SELECT
        Id                       = B.Id,
        DirectoryId              = B.DirectoryId,
        ExpiresAt                = B.ExpiresAt,
        PinnedAt                 = B.PinnedAt,
        Title                    = B.Title,
        Detail                   = B.Detail,
        RequiresAcknowledgement  = B.RequiresAcknowledgement,
        CategoryId               = BC.Id,
        CategoryName             = BC.[Name],
        CategoryIcon             = BC.Icon,
        CategoryColourCode       = BC.ColourCode,
        CreatedById              = B.CreatedById,
        CreatedByName            = COALESCE(CBUN.[Name], CBU.UserName),
        CreatedByIpAddress       = B.CreatedByIpAddress,
        CreatedAt                = B.CreatedAt,
        LastModifiedById         = B.LastModifiedById,
        LastModifiedByName       = COALESCE(MBUN.[Name], MBU.UserName),
        LastModifiedByIpAddress  = B.LastModifiedByIpAddress,
        LastModifiedAt           = B.LastModifiedAt,
        Version                  = B.Version,
        -- Attachments live directly in the bulletin's root directory (flat —
        -- the UI doesn't expose subdirectories), so this counts documents at
        -- DirectoryId without recursing into any sub-directory tree.
        AttachmentCount          = (
            SELECT COUNT(*) FROM dbo.Documents D
            WHERE D.DirectoryId = B.DirectoryId AND D.IsDeleted = 0
        )
    FROM dbo.Bulletins         B
    JOIN dbo.BulletinCategories BC  ON BC.Id = B.CategoryId
    JOIN dbo.Users              CBU ON CBU.Id = B.CreatedById
    JOIN dbo.Users              MBU ON MBU.Id = B.LastModifiedById
    OUTER APPLY dbo.fn_person_get_name(CBU.PersonId, 2, 0, 0) AS CBUN
    OUTER APPLY dbo.fn_person_get_name(MBU.PersonId, 2, 0, 0) AS MBUN
    WHERE B.Id = @bulletinId;

    -- 2) Audiences.
    SELECT
        Id               = BA.Id,
        AudienceKind     = BA.AudienceKind,
        StudentGroupId   = BA.StudentGroupId,
        StudentGroupName = SG.[Description]
    FROM dbo.BulletinAudiences BA
    LEFT JOIN dbo.StudentGroups SG ON SG.Id = BA.StudentGroupId
    WHERE BA.BulletinId = @bulletinId
    ORDER BY BA.AudienceKind, SG.[Description];

    -- 3) Ack rollup. NULL columns when the bulletin doesn't require ack — the
    --    DTO renders this as 'not applicable' rather than '0 / false'.
    SELECT
        AcknowledgedCount = CASE WHEN B.RequiresAcknowledgement = 1
            THEN (SELECT COUNT(*) FROM dbo.BulletinAcknowledgements WHERE BulletinId = B.Id)
            ELSE NULL END,
        HasAcknowledged   = CASE WHEN B.RequiresAcknowledgement = 1
            THEN CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.BulletinAcknowledgements
                    WHERE BulletinId = B.Id AND UserId = @currentUserId
                ) THEN 1 ELSE 0 END AS BIT)
            ELSE NULL END
    FROM dbo.Bulletins B
    WHERE B.Id = @bulletinId;
END;
