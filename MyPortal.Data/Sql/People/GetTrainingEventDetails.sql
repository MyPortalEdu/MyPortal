
SELECT
    te.[Id]                            AS [Id],
    te.[TrainingCourseId]              AS [TrainingCourseId],
    COALESCE(tc.[Name], tc.[Description]) AS [CourseName],
    de.[Subject]                       AS [Title],
    de.[StartTime]                     AS [StartTime],
    de.[EndTime]                       AS [EndTime],
    de.[Location]                      AS [Location],
    te.[Trainer]                       AS [Trainer],
    te.[Provider]                      AS [Provider],
    te.[Hours]                         AS [Hours],
    te.[Capacity]                      AS [Capacity],
    de.[Description]                   AS [Notes]
FROM [dbo].[TrainingEvents] te
    INNER JOIN [dbo].[DiaryEvents] de ON de.[Id] = te.[DiaryEventId]
    LEFT JOIN [dbo].[TrainingCourses] tc ON tc.[Id] = te.[TrainingCourseId]
WHERE te.[Id] = @id AND te.[IsDeleted] = 0;
