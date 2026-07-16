-- Staff-only summary for the staff picker. Joins StaffMembers so we only return
-- people who are actually staff (excludes students/contacts even when the same
-- Person row is also linked to one of those subtype tables).
-- Id is StaffMember.Id (the key for staff profile / CRUD endpoints); PersonId is
-- exposed alongside for callers writing person-FK columns (HeadTeacherId etc.).
--
-- Status is the employment-derived lifecycle badge (Active / Future / Leaver /
-- None), so QueryKit's list grid can sort and filter on it (it defaults to
-- Active). Soft-deleted rows are already excluded here, so 'Archived' never
-- arises.
--
-- Status comes from a CROSS APPLY, NOT a SELECT-list CASE alias. QueryKit
-- injects filter predicates into the inner WHERE on the bare column name
-- (e.g. `... AND [Status] = @p`), and a SELECT alias isn't in scope in WHERE —
-- that fails with "Invalid column name 'Status'". A CROSS APPLY column IS a real
-- table-source column, so the unqualified reference resolves. (No CTE — the
-- template is nested in parens for paging.) The spell tests are inline correlated
-- EXISTS, mirroring usp_staff_member_get_header_by_id.
SELECT
    SM.[Id],
    SM.[PersonId],
    SM.[Code],
    P.[Title],
    P.[FirstName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    S.[Status]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON SM.PersonId = P.Id
    CROSS APPLY (
        SELECT CASE
            WHEN EXISTS (
                SELECT 1 FROM [dbo].[StaffEmployments] E
                WHERE E.StaffMemberId = SM.Id AND E.IsDeleted = 0
                  AND CAST(E.StartDate AS date) <= CAST(SYSUTCDATETIME() AS date)
                  AND (E.EndDate IS NULL OR CAST(E.EndDate AS date) >= CAST(SYSUTCDATETIME() AS date))
            ) THEN 'Active'
            WHEN EXISTS (
                SELECT 1 FROM [dbo].[StaffEmployments] E
                WHERE E.StaffMemberId = SM.Id AND E.IsDeleted = 0
                  AND CAST(E.StartDate AS date) > CAST(SYSUTCDATETIME() AS date)
            ) THEN 'Future'
            WHEN EXISTS (
                SELECT 1 FROM [dbo].[StaffEmployments] E
                WHERE E.StaffMemberId = SM.Id AND E.IsDeleted = 0
            ) THEN 'Leaver'
            ELSE 'None'
        END AS [Status]
    ) S
WHERE SM.IsDeleted = 0
  AND P.IsDeleted = 0
