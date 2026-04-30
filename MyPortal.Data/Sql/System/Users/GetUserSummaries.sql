SELECT
    U.Id,
    U.CreatedAt,
    U.PersonId,
    U.UserType,
    U.IsEnabled,
    U.IsSystem,
    P.Name AS PersonFullName,
    U.UserName AS Username,
    U.Email,
    U.PhoneNumber,
    U.TwoFactorEnabled,
    U.LockoutEnabled
FROM dbo.Users U
OUTER APPLY dbo.fn_person_get_name(U.PersonId, 3, 0, 0) P