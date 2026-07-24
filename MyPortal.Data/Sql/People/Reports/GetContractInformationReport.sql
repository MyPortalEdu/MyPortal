-- Contract Information report: every open contract per staff member as at @effectiveDate, with its
-- terms (type, FTE, hours/weeks), role and pay point.
--
-- Parameters:
--   @staffType     : 'All' | 'Teaching' | 'Support'.
--   @effectiveDate : the date the contract must be open on.

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    ST.[Description] AS [ServiceTerm],
    SC.[PostTitle]   AS [PostTitle],
    SR.[Description] AS [Role],
    CT.[Description] AS [ContractType],
    SC.[Fte]          AS [Fte],
    SC.[HoursPerWeek] AS [HoursPerWeek],
    SC.[WeeksPerYear] AS [WeeksPerYear],
    PS.[Code]  AS [PayScale],
    PSP.[Code] AS [PayPoint],
    CAST(SC.[StartDate] AS date) AS [StartDate],
    CAST(SC.[EndDate] AS date)   AS [EndDate]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    INNER JOIN [dbo].[StaffEmployments] E ON E.[StaffMemberId] = SM.[Id] AND E.[IsDeleted] = 0
    INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
    LEFT JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SC.[ServiceTermId]
    LEFT JOIN [dbo].[StaffRoles] SR ON SR.[Id] = SC.[StaffRoleId]
    LEFT JOIN [dbo].[ContractTypes] CT ON CT.[Id] = SC.[ContractTypeId]
    LEFT JOIN [dbo].[PayScales] PS ON PS.[Id] = SC.[PayScaleId]
    LEFT JOIN [dbo].[PayScalePoints] PSP ON PSP.[Id] = SC.[PayScalePointId]
WHERE SM.[IsDeleted] = 0 AND P.[IsDeleted] = 0
  AND SC.[StartDate] <= @effectiveDate
  AND (SC.[EndDate] IS NULL OR CAST(SC.[EndDate] AS date) >= @effectiveDate)
  AND (E.[EndDate] IS NULL OR CAST(E.[EndDate] AS date) >= @effectiveDate)
  AND (@staffType = N'All'
       OR (@staffType = N'Teaching' AND SM.[IsTeachingStaff] = 1)
       OR (@staffType = N'Support'  AND SM.[IsTeachingStaff] = 0))
ORDER BY [StaffName];
