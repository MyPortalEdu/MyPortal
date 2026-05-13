-- Paged feed of bulletins visible to the current caller. Visibility matches
-- usp_bulletin_get_details_by_id exactly so a row appearing here will load on
-- click, and vice versa. QueryKit wraps this query for paging/filter/sort by
-- nesting it inside a subquery — which means we CAN'T use a CTE (WITH ...) at
-- the top of the template, because CTEs aren't valid inside parentheses. The
-- pupil-/parent-group lookups are inlined as correlated EXISTS subqueries
-- against the BulletinAudiences row (the correlation on StudentGroupId is
-- what makes this efficient — we only check membership for the exact group
-- the bulletin is targeting).
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
         ELSE NULL END AS HasAcknowledged,
    -- Attachments live directly in the bulletin's root directory (the UI
    -- doesn't expose subdirectories), so a flat COUNT against DirectoryId is
    -- correct in practice and avoids a recursive CTE in the feed hot path.
    (SELECT COUNT(*)
       FROM dbo.Documents D
      WHERE D.DirectoryId = B.DirectoryId
        AND D.IsDeleted = 0) AS AttachmentCount
FROM dbo.Bulletins         B
JOIN dbo.BulletinCategories BC  ON BC.Id = B.CategoryId
JOIN dbo.Users              CBU ON CBU.Id = B.CreatedById
OUTER APPLY dbo.fn_person_get_name(CBU.PersonId, 2, 0, 0) AS CBUN
-- Capture a single "now" for the whole row so expiry and membership-window
-- checks agree on a boundary and SYSUTCDATETIME() is evaluated once instead
-- of five times per row. QueryKit wraps this template in a subquery, so we
-- can't DECLARE at the top — CROSS APPLY scopes the value to the outer row
-- and is visible to the correlated EXISTS subqueries below.
CROSS APPLY (SELECT SYSUTCDATETIME() AS NowUtc) NW
WHERE
    -- Staff pinners see everything (admin oversight).
    (@isStaff = 1 AND @canPin = 1)
 OR -- Staff creators always see their own bulletins, even expired.
    (@isStaff = 1 AND @canEdit = 1 AND B.CreatedById = @currentUserId)
 OR -- Otherwise: not expired AND audience matches the caller's role.
    (
        (B.ExpiresAt IS NULL OR B.ExpiresAt > NW.NowUtc)
        AND EXISTS (
            SELECT 1
            FROM dbo.BulletinAudiences BA
            WHERE BA.BulletinId = B.Id
              AND (
                    (@isStaff  = 1 AND BA.AudienceKind = 1)
                 OR (@isPupil  = 1 AND BA.AudienceKind = 2)
                 OR (@isParent = 1 AND BA.AudienceKind = 3)
                 OR (@isPupil  = 1 AND BA.AudienceKind = 4 AND EXISTS (
                        SELECT 1
                        FROM dbo.StudentGroupMemberships SGM
                        JOIN dbo.Students                S   ON S.Id = SGM.StudentId
                        JOIN dbo.Users                   U   ON U.PersonId = S.PersonId
                        WHERE U.Id = @currentUserId
                          AND SGM.StudentGroupId = BA.StudentGroupId
                          AND SGM.StartDate <= NW.NowUtc
                          AND (SGM.EndDate IS NULL OR SGM.EndDate > NW.NowUtc)
                    ))
                 OR (@isParent = 1 AND BA.AudienceKind = 4 AND EXISTS (
                        SELECT 1
                        FROM dbo.StudentGroupMemberships     SGM
                        JOIN dbo.Students                    S   ON S.Id   = SGM.StudentId
                        JOIN dbo.StudentContactRelationships SCR ON SCR.StudentId = S.Id
                        JOIN dbo.Contacts                    C   ON C.Id   = SCR.ContactId
                        JOIN dbo.Users                       U   ON U.PersonId = C.PersonId
                        WHERE U.Id = @currentUserId
                          AND SGM.StudentGroupId = BA.StudentGroupId
                          AND SGM.StartDate <= NW.NowUtc
                          AND (SGM.EndDate IS NULL OR SGM.EndDate > NW.NowUtc)
                    ))
              )
        )
    )
