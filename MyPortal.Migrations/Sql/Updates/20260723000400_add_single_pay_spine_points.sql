-- ============================================================================
-- Let a pay spine belong to the service term, not just to a pay scale.
--
-- Two shapes exist in schools and only one of them was representable:
--
--   * Separate scales (SinglePaySpine = 0) — teachers. MPS 1-6 and UPS 1-3 are
--     distinct ranges; point 1 is worth different money on each. Points belong
--     to the scale, which owns its own range AND interval.
--
--   * A single spine (SinglePaySpine = 1) — NJC support staff. One spine of
--     SCP 1-43, each point worth one salary. Grades are overlapping WINDOWS
--     onto it: Grade 3 = SCP 5-11, Grade 4 = SCP 9-17, and SCP 9 is the same
--     money on either. Points belong to the SERVICE TERM; a scale only carries
--     the window bounds.
--
-- Without term-owned points the overlap duplicates SCP 9-11 as unrelated rows
-- that drift apart at the next pay award.
--
-- So PayScalePoints.PayScaleId becomes nullable and gains a ServiceTermId
-- sibling, with exactly one of the two set. StaffContracts.PayScalePointId is
-- untouched either way, so no contract data moves.
-- ============================================================================

-- ============================================================================
-- PayScales.PointInterval — only used when the scale owns its own points.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScales') AND name = N'PointInterval'
)
BEGIN
    ALTER TABLE [dbo].[PayScales] ADD [PointInterval] decimal(6,2) NULL;
END
GO

-- Inherit whatever the term was generating at, so existing scales regenerate
-- identically on their first save.
UPDATE PS
SET PS.[PointInterval] = ISNULL(ST.[PointInterval], 1.00)
FROM [dbo].[PayScales] PS
INNER JOIN [dbo].[ServiceTerms] ST ON ST.[Id] = PS.[ServiceTermId]
WHERE PS.[PointInterval] IS NULL
  AND PS.[MinimumPoint] IS NOT NULL;
GO

-- ============================================================================
-- PayScalePoints — ownership by scale OR service term
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScalePoints') AND name = N'ServiceTermId'
)
BEGIN
    ALTER TABLE [dbo].[PayScalePoints] ADD [ServiceTermId] uniqueidentifier NULL;
END
GO

-- The unique index has to go before the column it covers can be made nullable.
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PayScalePoints_PayScaleId_PointValue'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
    DROP INDEX [UX_PayScalePoints_PayScaleId_PointValue] ON [dbo].[PayScalePoints];
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayScalePoints_PayScaleId'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
    DROP INDEX [IX_PayScalePoints_PayScaleId] ON [dbo].[PayScalePoints];
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PayScalePoints') AND name = N'PayScaleId' AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[PayScalePoints] ALTER COLUMN [PayScaleId] uniqueidentifier NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PayScalePoints_ServiceTermId_ServiceTerms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
ALTER TABLE [dbo].[PayScalePoints]
    ADD CONSTRAINT [FK_PayScalePoints_ServiceTermId_ServiceTerms]
    FOREIGN KEY ([ServiceTermId]) REFERENCES [dbo].[ServiceTerms]([Id]);
END
GO

-- A point hangs off one owner or the other, never both and never neither.
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PayScalePoints_OneOwner')
BEGIN
ALTER TABLE [dbo].[PayScalePoints]
    ADD CONSTRAINT [CK_PayScalePoints_OneOwner]
    CHECK (([PayScaleId] IS NULL AND [ServiceTermId] IS NOT NULL)
        OR ([PayScaleId] IS NOT NULL AND [ServiceTermId] IS NULL));
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PayScalePoints_PayScaleId_PointValue'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
CREATE UNIQUE INDEX [UX_PayScalePoints_PayScaleId_PointValue]
    ON [dbo].[PayScalePoints]([PayScaleId], [PointValue])
    WHERE [PayScaleId] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PayScalePoints_ServiceTermId_PointValue'
    AND object_id = OBJECT_ID(N'[dbo].[PayScalePoints]'))
BEGIN
CREATE UNIQUE INDEX [UX_PayScalePoints_ServiceTermId_PointValue]
    ON [dbo].[PayScalePoints]([ServiceTermId], [PointValue])
    WHERE [ServiceTermId] IS NOT NULL;
END
GO

-- ============================================================================
-- The seeded scales are all teacher scales on a term with SinglePaySpine = 0,
-- so their points stay scale-owned. Nothing to migrate; the check constraint
-- above already holds for every existing row.
-- ============================================================================
