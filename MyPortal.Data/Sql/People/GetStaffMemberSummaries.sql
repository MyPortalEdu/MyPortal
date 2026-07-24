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
-- Role and EmploymentStartDate come from a single OUTER APPLY that picks the member's primary
-- spell (a current spell wins, else the most recent) and, within it, the latest-starting contract's
-- job role. Like Status they are apply columns, so QueryKit can sort/filter on the bare names.
SELECT
    SM.[Id],
    SM.[PersonId],
    SM.[Code],
    P.[Title],
    P.[FirstName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    P.[Gender],
    P.[Dob] AS [DateOfBirth],
    E.[Role],
    E.[EmploymentStartDate],
    E.[StartDateOnly],
    S.[Status],
    SN.[SearchName]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON SM.PersonId = P.Id
    -- All name parts concatenated so the Name column filter matches first/preferred, not just
    -- surname (an apply column, like Status, so QueryKit can filter it by the bare name).
    CROSS APPLY (
        SELECT LTRIM(RTRIM(
            COALESCE(P.[FirstName], '') + ' ' + COALESCE(P.[LastName], '') + ' ' +
            COALESCE(P.[PreferredFirstName], '') + ' ' + COALESCE(P.[PreferredLastName], '')
        )) AS [SearchName]
    ) SN
    OUTER APPLY (
        SELECT TOP 1
            EM.[StartDate] AS [EmploymentStartDate],
            -- Date-only copy so a date filter compares day-to-day (StartDate carries a time).
            CAST(EM.[StartDate] AS date) AS [StartDateOnly],
            (
                SELECT TOP 1 SR.[Description]
                FROM [dbo].[StaffContracts] SC
                    LEFT JOIN [dbo].[StaffRoles] SR ON SR.Id = SC.StaffRoleId
                WHERE SC.StaffEmploymentId = EM.Id AND SC.IsDeleted = 0
                ORDER BY SC.StartDate DESC
            ) AS [Role]
        FROM [dbo].[StaffEmployments] EM
        WHERE EM.StaffMemberId = SM.Id AND EM.IsDeleted = 0
        ORDER BY
            CASE
                WHEN CAST(EM.StartDate AS date) <= CAST(SYSUTCDATETIME() AS date)
                     AND (EM.EndDate IS NULL OR CAST(EM.EndDate AS date) >= CAST(SYSUTCDATETIME() AS date))
                THEN 0 ELSE 1
            END,
            EM.StartDate DESC
    ) E
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
