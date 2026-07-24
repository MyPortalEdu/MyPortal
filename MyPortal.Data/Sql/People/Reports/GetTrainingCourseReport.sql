-- Training Course report: the attendees (staff holding a record) for one training course.
--
-- Parameters:
--   @trainingCourseId : the course.

SELECT
    SM.[Code] AS [StaffCode],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName]) + N', ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName])
    )) AS [StaffName],
    S.[Description] AS [Status],
    CAST(C.[CompletedDate] AS date) AS [CompletedDate],
    CAST(C.[ExpiryDate] AS date)    AS [ExpiryDate],
    C.[Hours] AS [Hours]
FROM [dbo].[TrainingCertificates] C
    INNER JOIN [dbo].[StaffMembers] SM ON SM.[Id] = C.[StaffMemberId] AND SM.[IsDeleted] = 0
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    LEFT JOIN [dbo].[TrainingCertificateStatus] S ON S.[Id] = C.[TrainingCertificateStatusId]
WHERE C.[TrainingCourseId] = @trainingCourseId
ORDER BY [StaffName];
