-- Scheduled increments with their service term's identity and who set them up.
-- @serviceTermId : a specific term, or NULL for all terms (the due worklist).
-- @statusScheduledOnly : 1 to return only still-scheduled runs.
-- @dueBy : when >= 0 rows are also filtered to EffectiveDate <= @dueBy (the due list); NULL = no filter.

SELECT
    SI.[Id],
    SI.[ServiceTermId],
    ST.[Code] AS [ServiceTermCode],
    ST.[Description] AS [ServiceTermDescription],
    SI.[EffectiveDate],
    SI.[Status],
    SI.[CompletedAt],
    SI.[AppliedCount],
    SI.[CreatedAt],
    U.[UserName] AS [ScheduledBy]
FROM [dbo].[ScheduledIncrements] SI
    INNER JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = SI.[ServiceTermId]
    LEFT JOIN [dbo].[Users] U ON U.[Id] = SI.[CreatedById]
WHERE (@serviceTermId IS NULL OR SI.[ServiceTermId] = @serviceTermId)
  AND (@statusScheduledOnly = 0 OR SI.[Status] = 'Scheduled')
  AND (@dueBy IS NULL OR SI.[EffectiveDate] <= @dueBy)
ORDER BY SI.[EffectiveDate], ST.[Code];
