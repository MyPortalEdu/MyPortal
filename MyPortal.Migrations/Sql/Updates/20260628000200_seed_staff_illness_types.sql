-- ============================================================================
-- Seed StaffIllnessTypes for the Absences & Leave area. The StaffAbsences /
-- StaffAbsenceTypes / StaffIllnessTypes tables already exist (base schema) and
-- absence types are seeded via the CBDS lookup migration; only the illness-type
-- categories (used when an absence is sickness) were never seeded. Idempotent.
-- ============================================================================

MERGE INTO [dbo].[StaffIllnessTypes] AS Target
    USING (VALUES
    ('C5A1B200-0000-4000-8000-000000000001', 'Cold / Flu'),
    ('C5A1B200-0000-4000-8000-000000000002', 'Stomach / Digestive'),
    ('C5A1B200-0000-4000-8000-000000000003', 'Musculoskeletal (back, neck, limbs)'),
    ('C5A1B200-0000-4000-8000-000000000004', 'Stress / Anxiety / Depression'),
    ('C5A1B200-0000-4000-8000-000000000005', 'Headache / Migraine'),
    ('C5A1B200-0000-4000-8000-000000000006', 'Infection'),
    ('C5A1B200-0000-4000-8000-000000000007', 'Injury'),
    ('C5A1B200-0000-4000-8000-000000000008', 'Surgery / Hospital'),
    ('C5A1B200-0000-4000-8000-000000000009', 'Pregnancy related'),
    ('C5A1B200-0000-4000-8000-00000000000A', 'Other')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, IsSystem)
    VALUES (Id, Description, 1, 1)

    WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;
GO
