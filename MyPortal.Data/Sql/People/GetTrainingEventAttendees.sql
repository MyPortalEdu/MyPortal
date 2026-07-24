
SELECT
    sm.[Id]   AS [StaffMemberId],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(p.[PreferredLastName])), ''), p.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(p.[PreferredFirstName])), ''), p.[FirstName])
    )) AS [StaffName],
    sm.[Code] AS [StaffCode],
    a.[HasAttended] AS [HasAttended]
FROM [dbo].[DiaryEventAttendees] a
    INNER JOIN [dbo].[People] p ON p.[Id] = a.[PersonId]
    INNER JOIN [dbo].[StaffMembers] sm ON sm.[PersonId] = p.[Id] AND sm.[IsDeleted] = 0
WHERE a.[EventId] = @diaryEventId
ORDER BY [StaffName];
