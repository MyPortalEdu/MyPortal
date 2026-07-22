-- Contact-only summary for the contact list / picker. Joins Contacts so we only return people who
-- are actually contacts (excludes staff/students even when the same Person row is also linked to
-- one of those subtype tables).
-- Id is Contact.Id (the key for contact profile / CRUD endpoints); PersonId is exposed alongside
-- for callers writing person-FK columns.
--
-- LinkedStudentCount is how many students this contact is linked to, via StudentContactRelationships
-- (its ContactId FK). A correlated subquery so QueryKit's list grid can sort on the alias.
SELECT
    C.[Id],
    C.[PersonId],
    P.[Title],
    P.[FirstName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    C.[JobTitle],
    (
        SELECT COUNT(*)
        FROM [dbo].[StudentContactRelationships] SCR
        WHERE SCR.[ContactId] = C.[Id]
    ) AS [LinkedStudentCount]
FROM [dbo].[Contacts] C
    INNER JOIN [dbo].[People] P ON C.PersonId = P.Id
WHERE C.IsDeleted = 0
  AND P.IsDeleted = 0
