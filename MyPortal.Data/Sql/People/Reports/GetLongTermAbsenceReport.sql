-- Long-Term Absence Analysis report (pragmatic slice): sickness, maternity/paternity and pregnancy
-- absences that lost at least @minDays working days and overlap the period. Ordered longest first.
--
-- NOTE: SIMS' version is a three-year, FTE-weighted, school-calendar insurance return. This surfaces
-- the extended absences directly from working-days-lost; extend later if the insurance maths is needed.
--
-- Parameters:
--   @startDate : period start.
--   @endDate   : period end.
--   @minDays   : minimum working days lost to count as long-term.

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    AT.[Description] AS [AbsenceType],
    CAST(A.[StartDate] AS date) AS [StartDate],
    CAST(A.[EndDate] AS date)   AS [EndDate],
    A.[WorkingDaysLost]         AS [WorkingDaysLost]
FROM [dbo].[StaffAbsences] A
    INNER JOIN [dbo].[StaffMembers] SM ON SM.[Id] = A.[StaffMemberId] AND SM.[IsDeleted] = 0
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    INNER JOIN [dbo].[StaffAbsenceTypes] AT ON AT.[Id] = A.[AbsenceTypeId]
WHERE AT.[Code] IN (N'SIC', N'MAT', N'PRG')
  AND A.[StartDate] <= @endDate
  AND (A.[EndDate] IS NULL OR A.[EndDate] >= @startDate)
  AND COALESCE(A.[WorkingDaysLost], 0) >= @minDays
ORDER BY A.[WorkingDaysLost] DESC, [StaffName];
