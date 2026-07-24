-- Salary Information report: each staff member's current open contract as at @effectiveDate.
-- One row per open contract; FTE, full-time and actual salary, pay point and pension scheme.
--
-- Parameters:
--   @staffType     : 'All' | 'Teaching' | 'Support' (filters on StaffMembers.IsTeachingStaff).
--   @effectiveDate : the date the contract must be open on (started on/before, not yet ended).

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    ST.[Description] AS [ServiceTerm],
    SC.[PostTitle]   AS [PostTitle],
    PS.[Code]        AS [PayScale],
    PSP.[Code]       AS [PayPoint],
    SC.[Fte]         AS [Fte],
    CAST(SC.[AnnualSalary] / NULLIF(SC.[Fte], 0) AS decimal(18, 2)) AS [FullTimeSalary],
    SC.[AnnualSalary] AS [ActualSalary],
    SS.[Description]  AS [PensionScheme],
    CAST(SC.[StartDate] AS date) AS [ContractStartDate]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    INNER JOIN [dbo].[StaffEmployments] E ON E.[StaffMemberId] = SM.[Id] AND E.[IsDeleted] = 0
    INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
    LEFT JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SC.[ServiceTermId]
    LEFT JOIN [dbo].[PayScales] PS ON PS.[Id] = SC.[PayScaleId]
    LEFT JOIN [dbo].[PayScalePoints] PSP ON PSP.[Id] = SC.[PayScalePointId]
    LEFT JOIN [dbo].[SuperannuationSchemes] SS ON SS.[Id] = SC.[SuperannuationSchemeId]
WHERE SM.[IsDeleted] = 0 AND P.[IsDeleted] = 0
  AND SC.[StartDate] <= @effectiveDate
  AND (SC.[EndDate] IS NULL OR CAST(SC.[EndDate] AS date) >= @effectiveDate)
  AND (E.[EndDate] IS NULL OR CAST(E.[EndDate] AS date) >= @effectiveDate)
  AND (@staffType = N'All'
       OR (@staffType = N'Teaching' AND SM.[IsTeachingStaff] = 1)
       OR (@staffType = N'Support'  AND SM.[IsTeachingStaff] = 0))
ORDER BY [StaffName];
