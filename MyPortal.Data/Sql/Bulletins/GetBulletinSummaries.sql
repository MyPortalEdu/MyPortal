SELECT
    B.Id,
    B.ExpiresAt,
    B.Title,
    B.Detail,
    B.CreatedAt,
    B.IsPrivate,
    B.IsApproved,
    COALESCE(CBUN.Name, CBU.UserName) AS CreatedByName
FROM dbo.Bulletins B
INNER JOIN dbo.Users CBU ON CBU.Id = B.CreatedById
OUTER APPLY dbo.fn_person_get_name (CBU.PersonId, 2, 0, 0) AS CBUN
WHERE B.ExpiresAt > GETUTCDATE()