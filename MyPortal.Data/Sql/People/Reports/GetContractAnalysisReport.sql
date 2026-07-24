-- Contract Analysis report: service terms that have open contracts as at @effectiveDate, summarised
-- (contract count, distinct staff, teaching/support split, total FTE). One row per service term.
--
-- Parameters:
--   @staffType     : 'All' | 'Teaching' | 'Support'.
--   @effectiveDate : the date the contract must be open on.

SELECT
    COALESCE(ST.[Description], N'(No service term)') AS [ServiceTerm],
    COUNT(*)                     AS [ContractCount],
    COUNT(DISTINCT SM.[Id])      AS [StaffCount],
    SUM(CASE WHEN SM.[IsTeachingStaff] = 1 THEN 1 ELSE 0 END) AS [TeachingCount],
    SUM(CASE WHEN SM.[IsTeachingStaff] = 0 THEN 1 ELSE 0 END) AS [SupportCount],
    CAST(SUM(SC.[Fte]) AS decimal(18, 4)) AS [TotalFte]
FROM [dbo].[StaffMembers] SM
    INNER JOIN [dbo].[StaffEmployments] E ON E.[StaffMemberId] = SM.[Id] AND E.[IsDeleted] = 0
    INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
    LEFT JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SC.[ServiceTermId]
WHERE SM.[IsDeleted] = 0
  AND SC.[StartDate] <= @effectiveDate
  AND (SC.[EndDate] IS NULL OR CAST(SC.[EndDate] AS date) >= @effectiveDate)
  AND (E.[EndDate] IS NULL OR CAST(E.[EndDate] AS date) >= @effectiveDate)
  AND (@staffType = N'All'
       OR (@staffType = N'Teaching' AND SM.[IsTeachingStaff] = 1)
       OR (@staffType = N'Support'  AND SM.[IsTeachingStaff] = 0))
GROUP BY COALESCE(ST.[Description], N'(No service term)')
ORDER BY [ServiceTerm];
