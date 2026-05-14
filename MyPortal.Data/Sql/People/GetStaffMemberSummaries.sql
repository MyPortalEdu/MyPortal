-- Staff-only person summary for the staff picker. Joins StaffMembers so we
-- only return people who are actually staff (excludes students/contacts even
-- when the same Person row is also linked to one of those subtype tables).
-- Id is Person.Id so callers writing to person-FK columns (HeadTeacherId etc.)
-- can drop the value straight in.
SELECT
    P.[Id],
    SM.[Code],
    P.[Title],
    P.[FirstName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON SM.PersonId = P.Id
WHERE SM.IsDeleted = 0
  AND P.IsDeleted = 0
