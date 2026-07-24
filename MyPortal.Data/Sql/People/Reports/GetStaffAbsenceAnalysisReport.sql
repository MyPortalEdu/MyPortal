-- Staff Absence Analysis report: absences within a period summarised by service term. Each absence
-- is attributed to the staff member's most recent contract's service term.
--
-- Parameters:
--   @absenceTypeId : optional absence type filter (NULL = all types).
--   @startDate     : period start.
--   @endDate       : period end.

SELECT
    COALESCE(term.[ServiceTerm], N'(No service term)') AS [ServiceTerm],
    COUNT(*)                        AS [AbsenceCount],
    COUNT(DISTINCT A.[StaffMemberId]) AS [StaffCount],
    CAST(SUM(COALESCE(A.[WorkingDaysLost], 0)) AS decimal(18, 2)) AS [TotalWorkingDaysLost]
FROM [dbo].[StaffAbsences] A
    INNER JOIN [dbo].[StaffMembers] SM ON SM.[Id] = A.[StaffMemberId] AND SM.[IsDeleted] = 0
    OUTER APPLY (
        SELECT TOP 1 ST.[Description] AS [ServiceTerm]
        FROM [dbo].[StaffEmployments] E
            INNER JOIN [dbo].[StaffContracts] SC ON SC.[StaffEmploymentId] = E.[Id] AND SC.[IsDeleted] = 0
            LEFT JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SC.[ServiceTermId]
        WHERE E.[StaffMemberId] = A.[StaffMemberId] AND E.[IsDeleted] = 0
        ORDER BY SC.[StartDate] DESC
    ) term
WHERE (@absenceTypeId IS NULL OR A.[AbsenceTypeId] = @absenceTypeId)
  AND A.[StartDate] <= @endDate
  AND (A.[EndDate] IS NULL OR A.[EndDate] >= @startDate)
GROUP BY COALESCE(term.[ServiceTerm], N'(No service term)')
ORDER BY [ServiceTerm];
