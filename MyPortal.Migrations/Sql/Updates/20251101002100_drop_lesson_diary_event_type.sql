-- ============================================================================
-- Drop the seeded "Lesson" DiaryEventType row.
--
-- Lessons are backed by Sessions / SessionPeriods, not DiaryEvents — the row
-- existed only as a leftover from an earlier model. The matching enum value
-- (DiaryEventKind.Lesson = 2) and the C# constant are removed in the same
-- change; this migration cleans up the database side.
--
-- Guarded: if any DiaryEvent or DiaryEventTemplate is still pointing at this
-- type, fail loudly so the operator can clean up the stale rows before re-
-- running the migration. There's no FK on DiaryEvents.EventTypeId so the
-- DELETE wouldn't catch those, but DiaryEventTemplates does have one.
-- ============================================================================

DECLARE @lessonTypeId UNIQUEIDENTIFIER = '84E9DDA4-1BCB-4A2F-8082-FCE51DD04F23';

IF EXISTS (SELECT 1 FROM dbo.DiaryEvents WHERE EventTypeId = @lessonTypeId)
BEGIN
    THROW 50000,
        'Cannot drop Lesson DiaryEventType: one or more DiaryEvents still reference it. Reassign or delete them first.',
        1;
END

IF EXISTS (SELECT 1 FROM dbo.DiaryEventTemplates WHERE DiaryEventTypeId = @lessonTypeId)
BEGIN
    THROW 50000,
        'Cannot drop Lesson DiaryEventType: one or more DiaryEventTemplates still reference it. Reassign or delete them first.',
        1;
END

DELETE FROM dbo.DiaryEventTypes WHERE Id = @lessonTypeId;
GO
