-- Staff Training report: training records held by staff, optionally for one staff member, whose
-- completion date falls in the period (records not yet completed are included).
--
-- Parameters:
--   @staffMemberId : optional staff member filter (NULL = all staff).
--   @startDate     : period start.
--   @endDate       : period end.

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    COALESCE(TC.[Name], TC.[Description]) AS [Course],
    S.[Description] AS [Status],
    CAST(C.[CompletedDate] AS date) AS [CompletedDate],
    CAST(C.[ExpiryDate] AS date)    AS [ExpiryDate],
    C.[Hours]    AS [Hours],
    C.[Provider] AS [Provider]
FROM [dbo].[TrainingCertificates] C
    INNER JOIN [dbo].[StaffMembers] SM ON SM.[Id] = C.[StaffMemberId] AND SM.[IsDeleted] = 0
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    LEFT JOIN [dbo].[TrainingCourses] TC ON TC.[Id] = C.[TrainingCourseId]
    LEFT JOIN [dbo].[TrainingCertificateStatus] S ON S.[Id] = C.[TrainingCertificateStatusId]
WHERE (@staffMemberId IS NULL OR C.[StaffMemberId] = @staffMemberId)
  AND (C.[CompletedDate] IS NULL
       OR CAST(C.[CompletedDate] AS date) BETWEEN @startDate AND @endDate)
ORDER BY [StaffName], [Course];
