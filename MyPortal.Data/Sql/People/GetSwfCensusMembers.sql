
SELECT
    sm.[Id]                         AS [StaffMemberId],
    sm.[TeacherReferenceNumber]     AS [TeacherNumber],
    p.[LastName]                    AS [FamilyName],
    COALESCE(NULLIF(LTRIM(RTRIM(p.[PreferredFirstName])), ''), p.[FirstName]) AS [GivenName],
    p.[FormerSurname]               AS [FormerFamilyName],
    sm.[NiNumber]                   AS [NiNumber],
    p.[Gender]                      AS [Sex],
    CAST(p.[Dob] AS date)           AS [BirthDate],
    eth.[Code]                      AS [EthnicityCode],
    dis.[Code]                      AS [DisabilityCode],
    sm.[HasQts]                     AS [Qts],
    sm.[HasQtls]                    AS [Qtls],
    sm.[HasEyts]                    AS [Eyts],
    sm.[HasHlta]                    AS [Hlta],
    qr.[Code]                       AS [QtsRouteCode],
    sm.[IsSeniorLeadership]         AS [Slt],
    sm.[InductionCompletedDate]     AS [InductionCompletedDate],
    ct.[Code]                       AS [ContractTypeCode],
    CAST(c.[StartDate] AS date)     AS [ContractStart],
    CAST(c.[EndDate] AS date)       AS [ContractEnd],
    po.[SwrPostCode]                AS [PostCode],
    sr.[Code]                       AS [RoleCode],
    c.[DailyRate]                   AS [DailyRate],
    c.[AnnualSalary]                AS [BasePay],
    ps.[Code]                       AS [PayRangeCode],
    c.[SafeguardedSalary]           AS [SafeguardedSalary],
    c.[HoursPerWeek]                AS [FullTimeHoursPerWeek],
    c.[WeeksPerYear]                AS [WeeksPerYear],
    c.[Fte]                         AS [Fte],
    CAST(e.[StartDate] AS date)     AS [ArrivalDate],
    org.[Code]                      AS [OriginCode],
    dst.[Code]                      AS [DestinationCode],
    c.[Id]                          AS [ContractId]
FROM [dbo].[StaffMembers] sm
    INNER JOIN [dbo].[People] p ON p.[Id] = sm.[PersonId]
    INNER JOIN [dbo].[StaffEmployments] e ON e.[StaffMemberId] = sm.[Id] AND e.[IsDeleted] = 0
        AND e.[StartDate] <= @referenceDate
        AND (e.[EndDate] IS NULL OR CAST(e.[EndDate] AS date) >= @referenceDate)
    OUTER APPLY (
        SELECT TOP 1 c2.*
        FROM [dbo].[StaffContracts] c2
        WHERE c2.[StaffEmploymentId] = e.[Id] AND c2.[IsDeleted] = 0
          AND c2.[StartDate] <= @referenceDate
          AND (c2.[EndDate] IS NULL OR CAST(c2.[EndDate] AS date) >= @referenceDate)
        ORDER BY c2.[StartDate] DESC
    ) c
    LEFT JOIN [dbo].[ContractTypes] ct ON ct.[Id] = c.[ContractTypeId]
    LEFT JOIN [dbo].[Posts] po ON po.[Id] = c.[PostId]
    LEFT JOIN [dbo].[StaffRoles] sr ON sr.[Id] = c.[StaffRoleId]
    LEFT JOIN [dbo].[PayScales] ps ON ps.[Id] = c.[PayScaleId]
    LEFT JOIN [dbo].[Ethnicities] eth ON eth.[Id] = p.[EthnicityId]
    LEFT JOIN [dbo].[QtsRoutes] qr ON qr.[Id] = sm.[QtsRouteId]
    LEFT JOIN [dbo].[StaffOrigins] org ON org.[Id] = e.[OriginId]
    LEFT JOIN [dbo].[StaffDestinations] dst ON dst.[Id] = e.[DestinationId]
    OUTER APPLY (
        SELECT TOP 1 d.[Code]
        FROM [dbo].[StaffMemberDisabilities] smd
            INNER JOIN [dbo].[Disabilities] d ON d.[Id] = smd.[DisabilityId]
        WHERE smd.[StaffMemberId] = sm.[Id]
    ) dis
WHERE sm.[IsDeleted] = 0 AND p.[IsDeleted] = 0
ORDER BY p.[LastName], p.[FirstName];
