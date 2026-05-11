-- Paged feed of bulletins visible to the current caller. Visibility matches
-- usp_bulletin_get_details_by_id exactly so a row appearing here will load on
-- click, and vice versa. QueryKit wraps this query for paging/filter/sort; the
-- caller can sort to pin-first via SortOptions on PinnedAt DESC then CreatedAt DESC.
WITH PupilGroups AS (
    SELECT SGM.StudentGroupId
    FROM dbo.StudentGroupMemberships SGM
    JOIN dbo.Students                S   ON S.Id = SGM.StudentId
    JOIN dbo.Users                   U   ON U.PersonId = S.PersonId
    WHERE @isPupil = 1
      AND U.Id = @currentUserId
      AND SGM.StartDate <= SYSUTCDATETIME()
      AND (SGM.EndDate IS NULL OR SGM.EndDate > SYSUTCDATETIME())
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
      AND SGM.StartDate <= SYSUTCDATETIME()
      AND (SGM.EndDate IS NULL OR SGM.EndDate > SYSUTCDATETIME())
)
SELECT
    B.Id,
    B.ExpiresAt,
    B.PinnedAt,
    B.Title,
    B.Detail,
    B.CreatedAt,
    B.RequiresAcknowledgement,
    B.CategoryId,
    BC.[Name]        AS CategoryName,
    BC.Icon          AS CategoryIcon,
    BC.ColourCode    AS CategoryColourCode,
    COALESCE(CBUN.[Name], CBU.UserName) AS CreatedByName,
    -- NULL when the bulletin doesn't require ack; otherwise per-caller flag.
    CASE WHEN B.RequiresAcknowledgement = 1
         THEN CAST(CASE WHEN EXISTS (
                  SELECT 1 FROM dbo.BulletinAcknowledgements
                  WHERE BulletinId = B.Id AND UserId = @currentUserId
              ) THEN 1 ELSE 0 END AS BIT)
         ELSE NULL END AS HasAcknowledged
FROM dbo.Bulletins         B
JOIN dbo.BulletinCategories BC  ON BC.Id = B.CategoryId
JOIN dbo.Users              CBU ON CBU.Id = B.CreatedById
OUTER APPLY dbo.fn_person_get_name(CBU.PersonId, 2, 0, 0) AS CBUN
WHERE
    -- Staff pinners see everything (admin oversight).
    (@isStaff = 1 AND @canPin = 1)
 OR -- Staff creators always see their own bulletins, even expired.
    (@isStaff = 1 AND @canEdit = 1 AND B.CreatedById = @currentUserId)
 OR -- Otherwise: not expired AND audience matches the caller's role.
    (
        (B.ExpiresAt IS NULL OR B.ExpiresAt > SYSUTCDATETIME())
        AND EXISTS (
            SELECT 1
            FROM dbo.BulletinAudiences BA
            WHERE BA.BulletinId = B.Id
              AND (
                    (@isStaff  = 1 AND BA.AudienceKind = 1)
                 OR (@isPupil  = 1 AND BA.AudienceKind = 2)
                 OR (@isParent = 1 AND BA.AudienceKind = 3)
                 OR (@isPupil  = 1 AND BA.AudienceKind = 4
                                    AND BA.StudentGroupId IN (SELECT StudentGroupId FROM PupilGroups))
                 OR (@isParent = 1 AND BA.AudienceKind = 4
                                    AND BA.StudentGroupId IN (SELECT StudentGroupId FROM ParentGroups))
              )
        )
    )
