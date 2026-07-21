-- Student-only summary for the student list / picker. Joins Students so we only return people who
-- are actually students (excludes staff/contacts even when the same Person row is also linked to
-- one of those subtype tables).
-- Id is Student.Id (the key for student profile / CRUD endpoints); PersonId is exposed alongside
-- for callers writing person-FK columns.
--
-- Status is the admission-derived lifecycle badge (Active / Future / Leaver / None), so QueryKit's
-- list grid can sort and filter on it. Soft-deleted rows are already excluded here, so 'Archived'
-- never arises.
--
-- Status comes from a CROSS APPLY, NOT a SELECT-list CASE alias — QueryKit injects filter
-- predicates into the inner WHERE on the bare column name, and a SELECT alias isn't in scope there.
-- A CROSS APPLY column IS a real table-source column, so the unqualified reference resolves.
SELECT
    S.[Id],
    S.[PersonId],
    S.[AdmissionNumber],
    P.[Title],
    P.[FirstName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    ST.[Status]
FROM [dbo].[Students] S
    INNER JOIN [dbo].[People] P ON S.PersonId = P.Id
    CROSS APPLY (
        SELECT CASE
            WHEN S.[DateStarting] IS NULL THEN 'None'
            WHEN CAST(S.[DateStarting] AS date) > CAST(SYSUTCDATETIME() AS date) THEN 'Future'
            WHEN S.[DateLeaving] IS NOT NULL
                 AND CAST(S.[DateLeaving] AS date) < CAST(SYSUTCDATETIME() AS date) THEN 'Leaver'
            ELSE 'Active'
        END AS [Status]
    ) ST
WHERE S.IsDeleted = 0
  AND P.IsDeleted = 0
