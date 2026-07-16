-- ============================================================================
-- Seed Equality & Diversity lookups that have NO DfE CBDS equivalent.
--
-- Sexual Orientation has no CBDS code set, so it's seeded here with ONS / GSS
-- equality-monitoring categories. Gender Identity IS in the CBDS (CS120) and is
-- aligned to it below. Religion, Disability and Language are seeded from the CBDS
-- in the next migration (20260626000300_seed_cbds_lookups).
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

-- GenderIdentities (aligned to DfE CBDS CS120) -------------------------------
-- Display text is the friendly form of the CS120 categories; codes 10-99 ("self
-- describe" subcodes) collapse to "Prefer to self-describe" per the CBDS note.
MERGE INTO [dbo].[GenderIdentities] AS Target
    USING (VALUES
    ('A3B4C5D6-0120-4000-8000-000000000001', 'Man / Boy / Male'),
    ('A3B4C5D6-0120-4000-8000-000000000002', 'Woman / Girl / Female'),
    ('A3B4C5D6-0120-4000-8000-000000000003', 'Prefer to self-describe'),
    ('A3B4C5D6-0120-4000-8000-000000000004', 'Not known')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active) VALUES (Id, Description, 1)
    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = 1;
GO

-- Deactivate the previous ONS-style rows (replaced by CS120 above).
UPDATE [dbo].[GenderIdentities] SET [Active] = 0 WHERE [Id] NOT IN (
    'A3B4C5D6-0120-4000-8000-000000000001', 'A3B4C5D6-0120-4000-8000-000000000002',
    'A3B4C5D6-0120-4000-8000-000000000003', 'A3B4C5D6-0120-4000-8000-000000000004');
GO
