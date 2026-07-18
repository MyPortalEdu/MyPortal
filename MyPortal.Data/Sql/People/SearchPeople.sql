-- General person search for linking a Person to a user account. @like is the caller-built
-- contains pattern ('%term%'); the service guards against empty/too-short terms so this never
-- runs as an unfiltered table scan.
SELECT TOP 25
    P.[Id]            AS PersonId,
    P.[Title],
    P.[FirstName],
    P.[MiddleName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    P.[Dob]
FROM [dbo].[People] P
WHERE P.[IsDeleted] = 0
  AND (
        P.[FirstName] LIKE @like
     OR P.[LastName] LIKE @like
     OR P.[PreferredFirstName] LIKE @like
     OR P.[PreferredLastName] LIKE @like
     OR CONCAT(P.[FirstName], ' ', P.[LastName]) LIKE @like
  )
ORDER BY P.[LastName], P.[FirstName];
