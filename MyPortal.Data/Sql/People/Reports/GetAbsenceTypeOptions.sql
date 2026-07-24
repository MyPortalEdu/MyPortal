-- Absence types, for the absence reports' type filter.

SELECT
    AT.[Id] AS [Id],
    AT.[Description] AS [Name]
FROM [dbo].[StaffAbsenceTypes] AT
ORDER BY [Name];
