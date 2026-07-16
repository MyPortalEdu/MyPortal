-- ============================================================================
-- Seed staff document types.
--
-- The catalogue already had "Employment Contract" and the all-purpose "Other"
-- flagged Staff = 1. This adds the common staff/HR record types so the upload
-- type picker on the staff Documents area has a useful list. All are Staff = 1
-- and IsSystem = 1 (seeded catalogue, not user-managed). Insert-only MERGE so
-- it's re-runnable and won't clobber edits.
--
-- Column order mirrors the original DocumentTypes seed: Source(Id, Student,
-- Staff, Contact, General, Sen, Description).
-- ============================================================================

MERGE INTO [dbo].[DocumentTypes] AS Target
    USING (VALUES
    ('5D9555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 1, 0, 0, 0, 'DBS Certificate'),
    ('5DA555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 1, 0, 0, 0, 'Qualification'),
    ('5DB555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 1, 0, 0, 0, 'Reference'),
    ('5DC555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 1, 0, 0, 0, 'Right to Work')
    )
    AS Source (Id, Student, Staff, Contact, General, Sen, Description)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Student, Staff, Contact, General, IsSend, Active, IsSystem)
    VALUES (Id, Description, Student, Staff, Contact, General, Sen, 1, 1);
