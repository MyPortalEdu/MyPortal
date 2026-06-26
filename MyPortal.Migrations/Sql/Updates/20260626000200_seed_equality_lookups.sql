-- ============================================================================
-- Seed Equality & Diversity lookups that have NO DfE CBDS equivalent.
--
-- Sexual Orientation and Gender Identity aren't in the CBDS, so they're seeded
-- here with ONS / GSS equality-monitoring categories. Religion, Disability and
-- Language ARE in the CBDS and are seeded from it in the next migration
-- (20260626000300_seed_cbds_lookups), which is the source of truth for those.
--
-- MERGEs are upsert-safe so copy can be refined without orphaning rows.
-- ============================================================================

-- SexualOrientations ---------------------------------------------------------
MERGE INTO [dbo].[SexualOrientations] AS Target
    USING (VALUES
    ('A2B3C4D5-0002-4000-8000-000000000001', 'Heterosexual or straight'),
    ('A2B3C4D5-0002-4000-8000-000000000002', 'Gay or lesbian'),
    ('A2B3C4D5-0002-4000-8000-000000000003', 'Bisexual'),
    ('A2B3C4D5-0002-4000-8000-000000000004', 'Other'),
    ('A2B3C4D5-0002-4000-8000-000000000005', 'Prefer not to say')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active) VALUES (Id, Description, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO

-- GenderIdentities -----------------------------------------------------------
MERGE INTO [dbo].[GenderIdentities] AS Target
    USING (VALUES
    ('A3B4C5D6-0002-4000-8000-000000000001', 'Same as sex registered at birth'),
    ('A3B4C5D6-0002-4000-8000-000000000002', 'Different from sex registered at birth'),
    ('A3B4C5D6-0002-4000-8000-000000000003', 'Non-binary'),
    ('A3B4C5D6-0002-4000-8000-000000000004', 'Prefer to self-describe'),
    ('A3B4C5D6-0002-4000-8000-000000000005', 'Prefer not to say')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active) VALUES (Id, Description, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO
