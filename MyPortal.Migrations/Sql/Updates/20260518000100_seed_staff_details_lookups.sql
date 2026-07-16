-- ============================================================================
-- Seed lookup values for the staff details data model.
--
-- All MERGEs are upsert-safe: WHEN NOT MATCHED inserts the row, WHEN MATCHED
-- updates the mutable fields (description, colour). This lets us refine seed
-- copy or recolour over time without orphaning rows. Re-running the script is
-- a no-op once everything has converged.
--
-- Nationality is seeded with a small starter set — schools will extend this
-- as needed. Full ISO 3166 import is left to a later data load.
-- ============================================================================

-- Departments ----------------------------------------------------------------
MERGE INTO [dbo].[Departments] AS Target
    USING (VALUES
    ('D7E8F9A0-0001-4000-8000-000000000001', 'Senior Leadership Team', 'SLT',   '#7C3AED', 10),
    ('D7E8F9A0-0001-4000-8000-000000000002', 'Teaching Staff',         'TEACH', '#2563EB', 20),
    ('D7E8F9A0-0001-4000-8000-000000000003', 'Administration',         'ADMIN', '#059669', 30),
    ('D7E8F9A0-0001-4000-8000-000000000004', 'Finance',                'FIN',   '#D97706', 40),
    ('D7E8F9A0-0001-4000-8000-000000000005', 'IT Support',             'IT',    '#0891B2', 50),
    ('D7E8F9A0-0001-4000-8000-000000000006', 'Support Staff',          'SUP',   '#64748B', 60)
    )
    AS Source (Id, Description, Code, ColourCode, DisplayOrder)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, ColourCode, DisplayOrder, Active)
    VALUES (Id, Description, Code, ColourCode, DisplayOrder, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               Code = Source.Code,
               ColourCode = Source.ColourCode,
               DisplayOrder = Source.DisplayOrder;
GO

-- ContractTypes --------------------------------------------------------------
-- DEPRECATED: superseded by the CBDS seed (20260626000300_seed_cbds_lookups),
-- now the source of truth for ContractTypes. Left commented so new databases
-- don't reintroduce the old rows.
/*
MERGE INTO [dbo].[ContractTypes] AS Target
    USING (VALUES
    ('A8B7C6D5-0001-4000-8000-000000000001', 'Permanent'),
    ('A8B7C6D5-0001-4000-8000-000000000002', 'Fixed-term'),
    ('A8B7C6D5-0001-4000-8000-000000000003', 'Supply'),
    ('A8B7C6D5-0001-4000-8000-000000000004', 'Casual'),
    ('A8B7C6D5-0001-4000-8000-000000000005', 'Zero-hours')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
*/
GO

-- ServiceTerms ---------------------------------------------------------------
MERGE INTO [dbo].[ServiceTerms] AS Target
    USING (VALUES
    ('B2A3C4D5-0001-4000-8000-000000000001', 'Burgundy Book (Teachers)'),
    ('B2A3C4D5-0001-4000-8000-000000000002', 'NJC Green Book (Support Staff)'),
    ('B2A3C4D5-0001-4000-8000-000000000003', 'Soulbury'),
    ('B2A3C4D5-0001-4000-8000-000000000004', 'Sixth Form College Teachers'),
    ('B2A3C4D5-0001-4000-8000-000000000005', 'Independent School Standard Terms')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO

-- LeavingReasons -------------------------------------------------------------
-- DEPRECATED: superseded by 20260626000410_seed_workforce_result_lookups, which
-- refreshes LeavingReasons to the CBDS CS097 staff subset. Left commented so new
-- databases don't reintroduce the old rows.
/*
MERGE INTO [dbo].[LeavingReasons] AS Target
    USING (VALUES
    ('F1E2D3C4-0001-4000-8000-000000000001', 'Resignation'),
    ('F1E2D3C4-0001-4000-8000-000000000002', 'Retirement'),
    ('F1E2D3C4-0001-4000-8000-000000000003', 'End of Fixed-term'),
    ('F1E2D3C4-0001-4000-8000-000000000004', 'Redundancy'),
    ('F1E2D3C4-0001-4000-8000-000000000005', 'Dismissal'),
    ('F1E2D3C4-0001-4000-8000-000000000006', 'Other')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
*/
GO

-- DbsCheckTypes --------------------------------------------------------------
MERGE INTO [dbo].[DbsCheckTypes] AS Target
    USING (VALUES
    ('D5E6F7A8-0001-4000-8000-000000000001', 'Basic'),
    ('D5E6F7A8-0001-4000-8000-000000000002', 'Standard'),
    ('D5E6F7A8-0001-4000-8000-000000000003', 'Enhanced'),
    ('D5E6F7A8-0001-4000-8000-000000000004', 'Enhanced with Children''s Barred List')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO

-- RightToWorkDocumentTypes ---------------------------------------------------
MERGE INTO [dbo].[RightToWorkDocumentTypes] AS Target
    USING (VALUES
    ('E7F8A9B0-0001-4000-8000-000000000001', 'British Passport'),
    ('E7F8A9B0-0001-4000-8000-000000000002', 'Birth Certificate'),
    ('E7F8A9B0-0001-4000-8000-000000000003', 'EU Settled Status (Share Code)'),
    ('E7F8A9B0-0001-4000-8000-000000000004', 'Biometric Residence Permit'),
    ('E7F8A9B0-0001-4000-8000-000000000005', 'Work Visa'),
    ('E7F8A9B0-0001-4000-8000-000000000006', 'Other Eligible Document')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO

-- QualificationLevels (Ofqual framework) -------------------------------------
MERGE INTO [dbo].[QualificationLevels] AS Target
    USING (VALUES
    ('F9A0B1C2-0001-4000-8000-000000000001', 'Entry Level',  0),
    ('F9A0B1C2-0001-4000-8000-000000000002', 'Level 1',      1),
    ('F9A0B1C2-0001-4000-8000-000000000003', 'Level 2',      2),
    ('F9A0B1C2-0001-4000-8000-000000000004', 'Level 3',      3),
    ('F9A0B1C2-0001-4000-8000-000000000005', 'Level 4',      4),
    ('F9A0B1C2-0001-4000-8000-000000000006', 'Level 5',      5),
    ('F9A0B1C2-0001-4000-8000-000000000007', 'Level 6',      6),
    ('F9A0B1C2-0001-4000-8000-000000000008', 'Level 7',      7),
    ('F9A0B1C2-0001-4000-8000-000000000009', 'Level 8',      8)
    )
    AS Source (Id, Description, OfqualLevel)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, OfqualLevel, Active)
    VALUES (Id, Description, OfqualLevel, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               OfqualLevel = Source.OfqualLevel;
GO

-- MaritalStatuses ------------------------------------------------------------
MERGE INTO [dbo].[MaritalStatuses] AS Target
    USING (VALUES
    ('C5B6A7D8-0001-4000-8000-000000000001', 'Single'),
    ('C5B6A7D8-0001-4000-8000-000000000002', 'Married'),
    ('C5B6A7D8-0001-4000-8000-000000000003', 'Civil Partnership'),
    ('C5B6A7D8-0001-4000-8000-000000000004', 'Cohabiting'),
    ('C5B6A7D8-0001-4000-8000-000000000005', 'Separated'),
    ('C5B6A7D8-0001-4000-8000-000000000006', 'Divorced'),
    ('C5B6A7D8-0001-4000-8000-000000000007', 'Widowed'),
    ('C5B6A7D8-0001-4000-8000-000000000008', 'Prefer not to say')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO

-- InductionStatuses (ECT induction tracking) ---------------------------------
MERGE INTO [dbo].[InductionStatuses] AS Target
    USING (VALUES
    ('E9D8C7B6-0001-4000-8000-000000000001', 'Not Started', '#64748B'),
    ('E9D8C7B6-0001-4000-8000-000000000002', 'In Progress', '#D97706'),
    ('E9D8C7B6-0001-4000-8000-000000000003', 'Passed',      '#059669'),
    ('E9D8C7B6-0001-4000-8000-000000000004', 'Failed',      '#DC2626'),
    ('E9D8C7B6-0001-4000-8000-000000000005', 'Exempt',      '#6B7280')
    )
    AS Source (Id, Description, ColourCode)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, ColourCode, Active)
    VALUES (Id, Description, ColourCode, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               ColourCode = Source.ColourCode;
GO

-- Nationalities (starter set; extend as needed) ------------------------------
-- DEPRECATED: superseded by the CBDS seed (20260626000300_seed_cbds_lookups),
-- which loads the full nationality list. Left commented so new databases don't
-- reintroduce the old starter rows.
/*
MERGE INTO [dbo].[Nationalities] AS Target
    USING (VALUES
    ('B1A2C3D4-0001-4000-8000-000000000001', 'British',       'GBR'),
    ('B1A2C3D4-0001-4000-8000-000000000002', 'Irish',         'IRL'),
    ('B1A2C3D4-0001-4000-8000-000000000003', 'French',        'FRA'),
    ('B1A2C3D4-0001-4000-8000-000000000004', 'German',        'DEU'),
    ('B1A2C3D4-0001-4000-8000-000000000005', 'Polish',        'POL'),
    ('B1A2C3D4-0001-4000-8000-000000000006', 'Romanian',      'ROU'),
    ('B1A2C3D4-0001-4000-8000-000000000007', 'Indian',        'IND'),
    ('B1A2C3D4-0001-4000-8000-000000000008', 'Pakistani',     'PAK'),
    ('B1A2C3D4-0001-4000-8000-000000000009', 'American',      'USA'),
    ('B1A2C3D4-0001-4000-8000-00000000000A', 'Australian',    'AUS'),
    ('B1A2C3D4-0001-4000-8000-00000000000B', 'Canadian',      'CAN'),
    ('B1A2C3D4-0001-4000-8000-00000000000C', 'South African', 'ZAF'),
    ('B1A2C3D4-0001-4000-8000-00000000000D', 'Other',         NULL)
    )
    AS Source (Id, Description, IsoCode)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, IsoCode, Active)
    VALUES (Id, Description, IsoCode, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               IsoCode = Source.IsoCode;
*/
GO

-- PayScales ------------------------------------------------------------------
MERGE INTO [dbo].[PayScales] AS Target
    USING (VALUES
    ('C3D4E5F6-0001-4000-8000-000000000001', 'Main Pay Scale',           'MPS'),
    ('C3D4E5F6-0001-4000-8000-000000000002', 'Upper Pay Scale',          'UPS'),
    ('C3D4E5F6-0001-4000-8000-000000000003', 'Leadership Scale',         'LDR'),
    ('C3D4E5F6-0001-4000-8000-000000000004', 'Unqualified Teacher Scale','UNQ'),
    ('C3D4E5F6-0001-4000-8000-000000000005', 'NJC Support Staff Scale',  'NJC'),
    ('C3D4E5F6-0001-4000-8000-000000000006', 'Soulbury Scale',           'SOU'),
    ('C3D4E5F6-0001-4000-8000-000000000007', 'Locally Determined',       'LOC')
    )
    AS Source (Id, Description, Code)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description,
               Code = Source.Code;
GO
