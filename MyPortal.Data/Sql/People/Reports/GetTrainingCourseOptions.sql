-- Active training courses, for the Training Course report's course picker.

SELECT
    TC.[Id] AS [Id],
    COALESCE(TC.[Name], TC.[Description]) AS [Name]
FROM [dbo].[TrainingCourses] TC
WHERE TC.[Active] = 1
ORDER BY [Name];
