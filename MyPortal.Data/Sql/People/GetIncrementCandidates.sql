-- Current contracts on a service term that carry a pay-scale point, for the annual increment
-- routine. The service resolves the next point and the new salary; this just returns the state.
--
-- @serviceTermId : the term being incremented.
-- @asOf          : the increment's effective date (a spell must be live on it).

SELECT
    SC.[Id] AS [ContractId],
    E.[StaffMemberId],
    LTRIM(RTRIM(
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredFirstName])), ''), P.[FirstName]) + ' ' +
        COALESCE(NULLIF(LTRIM(RTRIM(P.[PreferredLastName])), ''), P.[LastName])
    )) AS [StaffName],
    SM.[Code] AS [StaffCode],
    SC.[PayScaleId],
    SC.[PayScalePointId],
    SC.[Fte],
    SC.[AnnualSalary],
    PT.[PointValue] AS [CurrentPointValue],
    PT.[Code] AS [CurrentPointCode],
    PS.[Code] AS [ScaleCode],
    PS.[Description] AS [ScaleDescription],
    PS.[MaximumPoint] AS [ScaleMaximumPoint]
FROM [dbo].[StaffContracts] SC
    INNER JOIN [dbo].[StaffEmployments] E ON E.[Id] = SC.[StaffEmploymentId] AND E.[IsDeleted] = 0
    INNER JOIN [dbo].[StaffMembers] SM ON SM.[Id] = E.[StaffMemberId] AND SM.[IsDeleted] = 0
    INNER JOIN [dbo].[People] P ON P.[Id] = SM.[PersonId]
    INNER JOIN [dbo].[PayScales] PS ON PS.[Id] = SC.[PayScaleId]
    INNER JOIN [dbo].[PayScalePoints] PT ON PT.[Id] = SC.[PayScalePointId]
WHERE SC.[IsDeleted] = 0
  AND SC.[ServiceTermId] = @serviceTermId
  AND SC.[PayScaleId] IS NOT NULL
  AND SC.[PayScalePointId] IS NOT NULL
  AND CAST(SC.[StartDate] AS date) <= @asOf
  AND (SC.[EndDate] IS NULL OR CAST(SC.[EndDate] AS date) >= @asOf)
ORDER BY [StaffName];
