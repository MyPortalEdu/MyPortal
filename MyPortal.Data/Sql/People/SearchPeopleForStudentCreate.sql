-- People search for the "new student" create flow. The office searches existing People (any
-- subtype) so someone already on file (a sibling's contact, a re-admitted former pupil, ...) gets a
-- student role attached to their existing Person rather than a duplicate Person row.
--
-- LEFT JOIN Students surfaces ExistingStudentId so the UI can block a duplicate student record and
-- offer a deep-link to the existing profile instead. @like is the caller-built contains pattern
-- ('%term%'); the service guards against empty/too-short terms so this never runs as an unfiltered
-- table scan.
SELECT TOP 25
    P.[Id]            AS PersonId,
    P.[Title],
    P.[FirstName],
    P.[MiddleName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    P.[Dob],
    S.[Id]            AS ExistingStudentId
FROM [dbo].[People] P
    LEFT JOIN [dbo].[Students] S ON S.[PersonId] = P.[Id] AND S.[IsDeleted] = 0
WHERE P.[IsDeleted] = 0
  AND (
        P.[FirstName] LIKE @like
     OR P.[LastName] LIKE @like
     OR P.[PreferredFirstName] LIKE @like
     OR P.[PreferredLastName] LIKE @like
     OR CONCAT(P.[FirstName], ' ', P.[LastName]) LIKE @like
  )
ORDER BY P.[LastName], P.[FirstName];
