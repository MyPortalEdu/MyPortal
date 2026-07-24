
SELECT
    te.[Id]                            AS [Id],
    COALESCE(tc.[Name], tc.[Description]) AS [CourseName],
    de.[Subject]                       AS [Title],
    de.[StartTime]                     AS [StartTime],
    de.[EndTime]                       AS [EndTime],
    de.[Location]                      AS [Location],
    te.[Trainer]                       AS [Trainer],
    (SELECT COUNT(*) FROM [dbo].[DiaryEventAttendees] a WHERE a.[EventId] = de.[Id]) AS [AttendeeCount],
    te.[Capacity]                      AS [Capacity]
FROM [dbo].[TrainingEvents] te
    INNER JOIN [dbo].[DiaryEvents] de ON de.[Id] = te.[DiaryEventId]
    LEFT JOIN [dbo].[TrainingCourses] tc ON tc.[Id] = te.[TrainingCourseId]
WHERE te.[IsDeleted] = 0
  AND de.[StartTime] <= @to
  AND COALESCE(de.[EndTime], de.[StartTime]) >= @from
ORDER BY de.[StartTime] DESC;
