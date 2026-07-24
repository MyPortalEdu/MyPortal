
SELECT
    a.[StaffMemberId]           AS [StaffMemberId],
    CAST(a.[StartDate] AS date) AS [FirstDay],
    CAST(a.[EndDate] AS date)   AS [LastDay],
    a.[WorkingDaysLost]         AS [WorkingDaysLost],
    at.[Code]                   AS [CategoryCode]
FROM [dbo].[StaffAbsences] a
    INNER JOIN [dbo].[StaffAbsenceTypes] at ON at.[Id] = a.[AbsenceTypeId]
WHERE at.[Code] = N'SIC'
  AND a.[StartDate] >= @absenceFrom
  AND a.[StartDate] <= @absenceTo
ORDER BY a.[StaffMemberId], a.[StartDate];
