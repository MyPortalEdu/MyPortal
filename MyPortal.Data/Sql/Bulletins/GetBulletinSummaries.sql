SELECT
    B.Id,
    B.ExpiresAt,
    B.Title,
    B.Detail,
    B.CreatedAt,
    B.IsPrivate,
    B.IsApproved,
    COALESCE(CBUN.Name, CBU.UserName) AS [CreatedByName]
FROM dbo.Bulletins B
INNER JOIN dbo.Users CBU ON CBU.Id = B.CreatedById
OUTER APPLY dbo.fn_person_get_name (CBU.PersonId, 2, 0, 0) AS CBUN
WHERE (@isStaff = 1 OR B.IsPrivate = 0)
  AND (
        @canApprove = 1
        OR (
            @isStaff = 0
            AND B.IsApproved = 1
            AND (B.ExpiresAt IS NULL OR B.ExpiresAt > @nowUtc)
        )
        OR (
            @isStaff = 1
            AND (@canView = 1 OR @canEdit = 1)
            AND (
                (@canEdit = 1 AND B.CreatedById = @currentUserId)
                OR (
                    B.IsApproved = 1
                    AND (B.ExpiresAt IS NULL OR B.ExpiresAt > @nowUtc)
                )
            )
        )
      )
