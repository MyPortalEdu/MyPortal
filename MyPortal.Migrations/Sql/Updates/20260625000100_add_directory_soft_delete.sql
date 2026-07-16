-- ============================================================================
-- Directory soft-delete.
--
-- Directories had no IsDeleted column, so a "soft" directory delete fell back to
-- a hard DELETE. When the directory still held soft-deleted Document rows
-- (Documents.DirectoryId FK), that hard delete tripped the FK — so a folder that
-- had ever contained a since-deleted file could not be removed.
--
-- Adding IsDeleted lets directory deletes be genuinely soft (the row is kept and
-- the FK stays satisfied), consistent with how person-scoped documents already
-- soft-delete. Guarded so the script is re-runnable.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Directories') AND name = N'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[Directories] ADD
        [IsDeleted] bit NOT NULL CONSTRAINT DF_Directories_IsDeleted DEFAULT (0);
END
GO
