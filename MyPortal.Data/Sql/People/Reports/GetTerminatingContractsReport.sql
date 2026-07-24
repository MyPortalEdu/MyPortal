-- Terminating Contracts report: contracts whose end date falls within [@startDate, @endDate].
--
-- Parameters:
--   @startDate : period start (inclusive).
--   @endDate   : period end (inclusive).

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    SC.[PostTitle]   AS [PostTitle],
    CT.[Description] AS [ContractType],
    ST.[Description] AS [ServiceTerm],
    SC.[Fte]         AS [Fte],
    CAST(SC.[EndDate] AS date) AS [EndDate]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    INNER JOIN [dbo].[StaffEmployments] E ON E.[StaffMemberId] = SM.[Id] AND E.[IsDeleted] = 0
    INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
    LEFT JOIN [dbo].[ContractTypes] CT ON CT.[Id] = SC.[ContractTypeId]
    LEFT JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SC.[ServiceTermId]
WHERE SM.[IsDeleted] = 0 AND P.[IsDeleted] = 0
  AND SC.[EndDate] IS NOT NULL
  AND CAST(SC.[EndDate] AS date) BETWEEN @startDate AND @endDate
ORDER BY [EndDate], [StaffName];
