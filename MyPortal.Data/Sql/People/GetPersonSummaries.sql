SELECT
    P.[Id],
    P.[Title],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    P.[FirstName],
    P.[MiddleName],
    P.[LastName],
    P.[PhotoId],
    P.[Gender],
    P.[Dob],
    P.[Deceased],
    P.[EthnicityId],
    CAST(
            (CASE WHEN EXISTS (SELECT 1 FROM dbo.Students     S  WHERE S.PersonId  = P.Id) THEN 1 ELSE 0 END)
            | (CASE WHEN EXISTS (SELECT 1 FROM dbo.StaffMembers SM WHERE SM.PersonId = P.Id) THEN 2 ELSE 0 END)
            | (CASE WHEN EXISTS (SELECT 1 FROM dbo.Contacts     C  WHERE C.PersonId  = P.Id) THEN 4 ELSE 0 END)
            | (CASE WHEN EXISTS (SELECT 1 FROM dbo.Agents       A  WHERE A.PersonId  = P.Id) THEN 8 ELSE 0 END)
            AS int
        ) AS [PersonType]
FROM [People] [P]