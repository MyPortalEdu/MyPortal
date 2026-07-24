-- Individual Absence report: all absences for one staff member over a period, optionally of one type.
--
-- Parameters:
--   @staffMemberId : the staff member.
--   @absenceTypeId : optional absence type filter (NULL = all types).
--   @startDate     : period start.
--   @endDate       : period end.

SELECT
    AT.[Description] AS [AbsenceType],
    IT.[Description] AS [IllnessType],
    CAST(A.[StartDate] AS date) AS [StartDate],
    CAST(A.[EndDate] AS date)   AS [EndDate],
    A.[WorkingDaysLost] AS [WorkingDaysLost],
    A.[HoursLost]       AS [HoursLost],
    A.[Notes]           AS [Notes]
FROM [dbo].[StaffAbsences] A
    LEFT JOIN [dbo].[StaffAbsenceTypes] AT ON AT.[Id] = A.[AbsenceTypeId]
    LEFT JOIN [dbo].[StaffIllnessTypes] IT ON IT.[Id] = A.[IllnessTypeId]
WHERE A.[StaffMemberId] = @staffMemberId
  AND (@absenceTypeId IS NULL OR A.[AbsenceTypeId] = @absenceTypeId)
  AND A.[StartDate] <= @endDate
  AND (A.[EndDate] IS NULL OR A.[EndDate] >= @startDate)
ORDER BY A.[StartDate];
