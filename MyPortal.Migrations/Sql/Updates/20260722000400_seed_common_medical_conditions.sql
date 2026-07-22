-- Seed a starting set of common school-relevant medical conditions (the lookup shipped with only
-- one row). Matched on Description so a school that has already added any of these keeps its own
-- row rather than getting a duplicate; new rows get a fresh Id and are Active.

MERGE INTO [dbo].[MedicalConditions] AS Target
    USING (VALUES
        (N'Asthma'),
        (N'Anaphylaxis'),
        (N'Food allergy'),
        (N'Hay fever'),
        (N'Eczema'),
        (N'Diabetes (Type 1)'),
        (N'Epilepsy'),
        (N'ADHD'),
        (N'Autism spectrum condition'),
        (N'Coeliac disease'),
        (N'Cystic fibrosis'),
        (N'Sickle cell disease'),
        (N'Migraine'),
        (N'Heart condition'),
        (N'Cerebral palsy'),
        (N'Hearing impairment'),
        (N'Visual impairment')
    ) AS Source ([Description])
    ON Target.[Description] = Source.[Description]
    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Description], [Active])
         VALUES (NEWID(), Source.[Description], 1);
