-- PersonFullName comes from an OUTER APPLY that aliases the column to its final name
-- (PersonFullName), NOT a SELECT-list alias. QueryKit injects filter/sort predicates into the
-- inner WHERE/ORDER BY on the bare column name (e.g. `... AND [PersonFullName] LIKE @p`), and a
-- SELECT alias isn't in scope in WHERE — that fails with "Invalid column name 'PersonFullName'".
-- An APPLY column IS a real table-source column, so the unqualified reference resolves. (Username
-- and Email filter fine even as SELECT aliases because they resolve to the real Users columns.)
SELECT
    U.Id,
    U.CreatedAt,
    U.PersonId,
    U.UserType,
    U.IsEnabled,
    U.IsSystem,
    P.PersonFullName,
    U.UserName AS Username,
    U.Email,
    U.PhoneNumber,
    U.TwoFactorEnabled,
    U.LockoutEnabled
FROM dbo.Users U
OUTER APPLY (
    SELECT Name AS PersonFullName
    FROM dbo.fn_person_get_name(U.PersonId, 3, 0, 0)
) P
