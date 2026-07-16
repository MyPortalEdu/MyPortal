-- ============================================================================
-- Add IsSystem to Directories, and flag the system Photos directory.
--
-- 20260707000000 parked person-photo documents in a dedicated directory
-- ('system-photos') so they never surface in a person's Documents tab. That
-- part holds by construction: the directory is a root that no per-entity
-- attachments browser is ever scoped to, so nothing can navigate to it.
--
-- What did NOT hold is protection from deletion. The photo *document type* is
-- seeded IsSystem = 1 and DocumentType implements ISystemEntity, so
-- EntityRepository refuses to update or delete it. The directory holding those
-- documents had no such marker — Directories had no IsSystem column at all, so
-- the row was an ordinary soft-deletable directory. Any future directory-admin
-- surface would list it and let it be deleted, orphaning every photo document
-- behind the Documents.DirectoryId FK. The safety of the lane rested on the
-- absence of a UI rather than on a constraint.
--
-- Directory now implements ISystemEntity, so EntityRepository.UpdateAsync and
-- DeleteAsync both throw SystemEntityException for rows flagged here. That
-- matches how the Photograph document type is already protected.
--
-- The DEFAULT is kept (unlike 20260515000000's IsLesson, which dropped it so
-- callers had to choose): 0 is unambiguously correct for a user-created
-- directory, and it keeps the earlier MERGE-based directory seeds in
-- 20251101000300 / 20260707000000 valid without amendment.
--
-- Idempotent: guarded column add + an UPDATE that is a no-op once applied.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Directories') AND name = N'IsSystem'
)
BEGIN
    ALTER TABLE [dbo].[Directories]
        ADD [IsSystem] BIT NOT NULL
            CONSTRAINT DF_Directories_IsSystem DEFAULT (0);
END
GO

-- Flag the system Photos directory seeded by 20260707000000. Scoped to that one
-- id: every other directory (incl. the seeded root) stays user-managed.
UPDATE [dbo].[Directories]
SET [IsSystem] = 1
WHERE [Id] = '0F0705D1-0000-4000-8000-000000000001'
  AND [IsSystem] = 0;
GO
