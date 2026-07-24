-- Staff HR compliance items: everything with an expiry that is past or near, plus core
-- safeguarding records that are entirely missing. One flat list the dashboard groups.
--
-- Parameters:
--   @today   : today's date (date).
--   @horizon : @today + N days; the "expiring soon" cut-off.
--
-- Scope: staff with a current or future employment spell only (a leaver's lapsed DBS is noise).
--
-- Kind: 'Expired' (due date already past), 'ExpiringSoon' (due within the horizon) or 'Missing'
-- (a required record not held at all). Only rows matching one of these are returned.
--
-- Per-source "latest wins": a staff member with a renewed DBS/RTW is judged on the newest one, so
-- an old lapsed record next to a valid current one does not raise a false flag.

WITH InScope AS (
    SELECT
        SM.[Id] AS [StaffMemberId],
        LTRIM(RTRIM(
            COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName]) + ' ' +
            COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName])
        )) AS [DisplayName],
        SM.[Code]
    FROM [dbo].[StaffMembers] SM
        INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    WHERE SM.[IsDeleted] = 0 AND P.[IsDeleted] = 0
      AND EXISTS (
          SELECT 1 FROM [dbo].[StaffEmployments] E
          WHERE E.[StaffMemberId] = SM.[Id] AND E.[IsDeleted] = 0
            AND (E.[EndDate] IS NULL OR CAST(E.[EndDate] AS date) >= @today)
      )
),
DbsLatest AS (
    SELECT [StaffMemberId], MAX(CAST([ExpiryDate] AS date)) AS [ExpiryDate]
    FROM [dbo].[DbsChecks]
    WHERE [IsDeleted] = 0 AND [ExpiryDate] IS NOT NULL
    GROUP BY [StaffMemberId]
),
RtwLatest AS (
    SELECT R.[StaffMemberId], R.[DocumentExpiryDate], R.[FollowUpDate]
    FROM [dbo].[RightToWorkChecks] R
    WHERE R.[IsDeleted] = 0
      AND R.[CheckDate] = (
          SELECT MAX(R2.[CheckDate]) FROM [dbo].[RightToWorkChecks] R2
          WHERE R2.[StaffMemberId] = R.[StaffMemberId] AND R2.[IsDeleted] = 0
      )
),
TrainingLatest AS (
    -- TrainingCertificates has no soft-delete column, so no IsDeleted filter here.
    SELECT TC.[StaffMemberId], TC.[TrainingCourseId], MAX(CAST(TC.[ExpiryDate] AS date)) AS [ExpiryDate]
    FROM [dbo].[TrainingCertificates] TC
    WHERE TC.[ExpiryDate] IS NOT NULL
    GROUP BY TC.[StaffMemberId], TC.[TrainingCourseId]
),
Items AS (
    -- DBS expiring / expired
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'Dbs' AS [Category], N'DBS certificate' AS [Detail], D.[ExpiryDate] AS [DueDate]
    FROM InScope I
        INNER JOIN DbsLatest D ON D.[StaffMemberId] = I.[StaffMemberId]
    WHERE D.[ExpiryDate] <= @horizon

    UNION ALL
    -- DBS missing entirely
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'Dbs', N'No DBS on record', NULL
    FROM InScope I
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[DbsChecks] D WHERE D.[StaffMemberId] = I.[StaffMemberId] AND D.[IsDeleted] = 0
    )

    UNION ALL
    -- Right to work — document expiry
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'RightToWork', N'Right-to-work document', CAST(R.[DocumentExpiryDate] AS date)
    FROM InScope I
        INNER JOIN RtwLatest R ON R.[StaffMemberId] = I.[StaffMemberId]
    WHERE R.[DocumentExpiryDate] IS NOT NULL AND CAST(R.[DocumentExpiryDate] AS date) <= @horizon

    UNION ALL
    -- Right to work — follow-up check due
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'RightToWork', N'Right-to-work follow-up check', CAST(R.[FollowUpDate] AS date)
    FROM InScope I
        INNER JOIN RtwLatest R ON R.[StaffMemberId] = I.[StaffMemberId]
    WHERE R.[FollowUpDate] IS NOT NULL AND CAST(R.[FollowUpDate] AS date) <= @horizon

    UNION ALL
    -- Right to work missing entirely
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'RightToWork', N'No right-to-work check on record', NULL
    FROM InScope I
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[RightToWorkChecks] R WHERE R.[StaffMemberId] = I.[StaffMemberId] AND R.[IsDeleted] = 0
    )

    UNION ALL
    -- Training certificate expiry
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'Training', COALESCE(C.[Description], N'Training'), T.[ExpiryDate]
    FROM InScope I
        INNER JOIN TrainingLatest T ON T.[StaffMemberId] = I.[StaffMemberId]
        LEFT JOIN [dbo].[TrainingCourses] C ON C.[Id] = T.[TrainingCourseId]
    WHERE T.[ExpiryDate] <= @horizon

    UNION ALL
    -- Contract ending soon (terminating contracts)
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'Contract',
        COALESCE(N'Contract ending — ' + SR.[Description], N'Contract ending'),
        CAST(SC.[EndDate] AS date)
    FROM InScope I
        INNER JOIN [dbo].[StaffEmployments] E ON E.[StaffMemberId] = I.[StaffMemberId] AND E.[IsDeleted] = 0
        INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
        LEFT JOIN [dbo].[StaffRoles] SR ON SR.[Id] = SC.[StaffRoleId]
    WHERE SC.[EndDate] IS NOT NULL
      AND CAST(SC.[EndDate] AS date) >= @today
      AND CAST(SC.[EndDate] AS date) <= @horizon

    UNION ALL
    -- Pre-employment (SCR) core checks incomplete
    SELECT I.[StaffMemberId], I.[DisplayName], I.[Code],
        'PreEmployment', N'Pre-employment checks incomplete', NULL
    FROM InScope I
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[StaffPreEmploymentChecks] PE
        WHERE PE.[StaffMemberId] = I.[StaffMemberId] AND PE.[IsDeleted] = 0
          AND PE.[IdentityCheckedDate] IS NOT NULL
          AND PE.[MedicalFitnessCheckedDate] IS NOT NULL
    )
)
SELECT
    [StaffMemberId],
    [DisplayName] AS [StaffName],
    [Code] AS [StaffCode],
    [Category],
    [Detail],
    [DueDate],
    CASE
        WHEN [DueDate] IS NULL THEN 'Missing'
        WHEN [DueDate] < @today THEN 'Expired'
        ELSE 'ExpiringSoon'
    END AS [Kind]
FROM Items
ORDER BY
    CASE
        WHEN [DueDate] IS NULL THEN 1                 -- Missing
        WHEN [DueDate] < @today THEN 0                -- Expired (most urgent)
        ELSE 2                                        -- ExpiringSoon
    END,
    [DueDate],
    [DisplayName];
