-- Staff-only summary for the staff picker. Joins StaffMembers so we only return
-- people who are actually staff (excludes students/contacts even when the same
-- Person row is also linked to one of those subtype tables).
-- Id is StaffMember.Id (the key for staff profile / CRUD endpoints); PersonId is
-- exposed alongside for callers writing person-FK columns (HeadTeacherId etc.).
SELECT
    SM.[Id],
    SM.[PersonId],
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
