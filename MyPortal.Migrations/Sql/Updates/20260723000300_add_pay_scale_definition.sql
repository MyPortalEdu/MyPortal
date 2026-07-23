-- ============================================================================
-- Make pay scales user-definable.
--
-- Until now PayScales and PayScalePoints were seed-only: a scale was a bare
-- lookup and its points were hand-listed in a migration. This gives a scale
-- the structure it needs to be maintained from Staff Setup:
--
--   * PayScales.ServiceTermId — a scale belongs to a service term, so the
--     teacher terms carry the teacher scales and the support terms carry the
--     support scales.
--
--   * PayScales.MinimumPoint / MaximumPoint — the scale's point range. Points
--     are generated across it using the service term's PointInterval, so the
--     grid is derived from three numbers rather than typed out row by row.
--     Nullable: a scale whose range has never been set generates no points,
--     which is the honest state for the NJC/Soulbury/Locally Determined
--     scales that were never seeded with any.
--
--   * PayScalePoints.PointValue — the numeric spine position (1.0, 1.5, 2.0).
--     This is what generation matches on, so regenerating a scale leaves the
--     existing points (and every contract pointing at them) untouched.
--
-- The service term keeps its own MinimumPoint/MaximumPoint/PointInterval: it
-- is the outer range every scale on the term must sit inside.
-- ============================================================================

-- ============================================================================
-- PayScales
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScales') AND name = N'ServiceTermId'
)
BEGIN
    ALTER TABLE [dbo].[PayScales] ADD [ServiceTermId] uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScales') AND name = N'MinimumPoint'
)
BEGIN
    ALTER TABLE [dbo].[PayScales] ADD [MinimumPoint] decimal(6,2) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScales') AND name = N'MaximumPoint'
)
BEGIN
    ALTER TABLE [dbo].[PayScales] ADD [MaximumPoint] decimal(6,2) NULL;
END
GO

-- ============================================================================
-- Backfill: attach the seeded scales to the service term that uses them.
-- Teacher scales sit on Burgundy Book; the support-staff and Soulbury scales
-- sit on their own terms. Locally Determined follows the support terms, being
-- the fallback for non-statutory support pay.
-- ============================================================================

UPDATE PS
SET PS.[ServiceTermId] = ST.[Id]
FROM [dbo].[PayScales] PS
INNER JOIN [dbo].[ServiceTerms] ST
    ON ST.[Code] = CASE PS.[Code]
                       WHEN 'MPS' THEN 'BURGUNDY'
                       WHEN 'UPS' THEN 'BURGUNDY'
                       WHEN 'LDR' THEN 'BURGUNDY'
                       WHEN 'UNQ' THEN 'BURGUNDY'
                       WHEN 'NJC' THEN 'NJC'
                       WHEN 'LOC' THEN 'NJC'
                       WHEN 'SOU' THEN 'SOULBURY'
                   END
WHERE PS.[ServiceTermId] IS NULL;
GO

-- Any scale the mapping above missed lands on the first service term rather
-- than being left orphaned by the NOT NULL below.
UPDATE [dbo].[PayScales]
SET [ServiceTermId] = (SELECT TOP 1 [Id] FROM [dbo].[ServiceTerms] ORDER BY [Code])
WHERE [ServiceTermId] IS NULL
  AND EXISTS (SELECT 1 FROM [dbo].[ServiceTerms]);
GO

-- Codes are required from here on; the seeded scales all have one.
UPDATE [dbo].[PayScales]
SET [Code] = LEFT(UPPER(REPLACE([Description], ' ', '')), 10)
WHERE [Code] IS NULL OR LTRIM(RTRIM([Code])) = '';
GO

IF EXISTS (SELECT 1 FROM [dbo].[PayScales])
   AND NOT EXISTS (SELECT 1 FROM [dbo].[PayScales] WHERE [ServiceTermId] IS NULL)
BEGIN
    ALTER TABLE [dbo].[PayScales] ALTER COLUMN [ServiceTermId] uniqueidentifier NOT NULL;
END
ELSE IF NOT EXISTS (SELECT 1 FROM [dbo].[PayScales])
BEGIN
    ALTER TABLE [dbo].[PayScales] ALTER COLUMN [ServiceTermId] uniqueidentifier NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[PayScales] WHERE [Code] IS NULL)
BEGIN
    ALTER TABLE [dbo].[PayScales] ALTER COLUMN [Code] nvarchar(10) NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PayScales_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PayScales]'))
BEGIN
ALTER TABLE [dbo].[PayScales]
    ADD CONSTRAINT [FK_PayScales_ServiceTermId_ServiceTerms]
    FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PayScales_ServiceTermId_Code'
    AND object_id = OBJECT_ID(N'[dbo].[PayScales]'))
BEGIN
CREATE UNIQUE INDEX [UX_PayScales_ServiceTermId_Code]
    ON [dbo].[PayScales]([ServiceTermId], [Code]);
END
GO

-- ============================================================================
-- PayScalePoints.PointValue
--
-- Widen Code first: a generated point takes its code from the scale code plus
-- the point value ('MPS7', 'NJC12.5'), which does not fit in 10 characters.
-- ============================================================================

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScalePoints') AND name = N'Code' AND max_length < 40
)
BEGIN
    ALTER TABLE [dbo].[PayScalePoints] ALTER COLUMN [Code] nvarchar(20) NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScalePoints') AND name = N'PointValue'
)
BEGIN
    ALTER TABLE [dbo].[PayScalePoints] ADD [PointValue] decimal(6,2) NULL;
END
GO

-- The seeded points are consecutive within their scale (M1..M6, L1..L18), so
-- their ordinal by DisplayOrder is their spine position.
WITH Ordered AS (
    SELECT [Id],
           ROW_NUMBER() OVER (PARTITION BY [PayScaleId] ORDER BY [DisplayOrder], [Code]) AS [Ordinal]
    FROM [dbo].[PayScalePoints]
    WHERE [PointValue] IS NULL
)
UPDATE P
SET P.[PointValue] = CAST(O.[Ordinal] AS decimal(6,2))
FROM [dbo].[PayScalePoints] P
INNER JOIN Ordered O ON O.[Id] = P.[Id];
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[PayScalePoints] WHERE [PointValue] IS NULL)
BEGIN
    ALTER TABLE [dbo].[PayScalePoints] ALTER COLUMN [PointValue] decimal(6,2) NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PayScalePoints_PayScaleId_PointValue'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
CREATE UNIQUE INDEX [UX_PayScalePoints_PayScaleId_PointValue]
    ON [dbo].[PayScalePoints]([PayScaleId], [PointValue]);
END
GO

-- ============================================================================
-- Backfill the scale ranges from the points that already exist, so the first
-- save of an existing scale regenerates exactly what is already there.
-- ============================================================================

UPDATE PS
SET PS.[MinimumPoint] = R.[MinPoint],
    PS.[MaximumPoint] = R.[MaxPoint]
FROM [dbo].[PayScales] PS
INNER JOIN (
    SELECT [PayScaleId], MIN([PointValue]) AS [MinPoint], MAX([PointValue]) AS [MaxPoint]
    FROM [dbo].[PayScalePoints]
    GROUP BY [PayScaleId]
) R ON R.[PayScaleId] = PS.[Id]
WHERE PS.[MinimumPoint] IS NULL AND PS.[MaximumPoint] IS NULL;
GO

-- Give the service terms the outer range their scales sit inside, and the
-- interval generation runs at. Scales with no points leave their term alone.
UPDATE ST
SET ST.[MinimumPoint] = R.[MinPoint],
    ST.[MaximumPoint] = R.[MaxPoint],
    ST.[PointInterval] = 1.00
FROM [dbo].[ServiceTerms] ST
INNER JOIN (
    SELECT PS.[ServiceTermId], MIN(PS.[MinimumPoint]) AS [MinPoint], MAX(PS.[MaximumPoint]) AS [MaxPoint]
    FROM [dbo].[PayScales] PS
    WHERE PS.[MinimumPoint] IS NOT NULL
    GROUP BY PS.[ServiceTermId]
) R ON R.[ServiceTermId] = ST.[Id]
WHERE ST.[MinimumPoint] IS NULL AND ST.[MaximumPoint] IS NULL;
GO
