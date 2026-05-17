SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Returns the first IDirectoryEntity row that holds a FK reference to the
-- given directory, or no rows if nothing references it. Used by
-- DirectoryService.DeleteAsync as a pre-flight check so the API can surface a
-- friendly "this directory belongs to X" error instead of letting the user
-- soft-delete a root directory that an entity still depends on. The FK
-- constraint on each owner table is the hard backstop for hard-deletes; this
-- proc covers soft-delete (which doesn't trip FKs) and produces nicer errors.
--
-- OwnerType is the entity-class name as a string, kept here rather than as an
-- enum so adding a new IDirectoryEntity owner is a one-line UNION ALL addition
-- with no schema change.
CREATE OR ALTER PROCEDURE [dbo].[usp_directory_get_referencing_owner]
    @directoryId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 OwnerType, OwnerId
    FROM (
        SELECT 'Bulletin'     AS OwnerType, Id AS OwnerId FROM dbo.Bulletins     WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'Agency'       AS OwnerType, Id AS OwnerId FROM dbo.Agencies      WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'Class'        AS OwnerType, Id AS OwnerId FROM dbo.Classes       WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'Document'     AS OwnerType, Id AS OwnerId FROM dbo.Documents     WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'HomeworkItem' AS OwnerType, Id AS OwnerId FROM dbo.HomeworkItems WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'LessonPlan'   AS OwnerType, Id AS OwnerId FROM dbo.LessonPlans   WHERE DirectoryId = @directoryId
        UNION ALL
        SELECT 'Person'       AS OwnerType, Id AS OwnerId FROM dbo.People        WHERE DirectoryId = @directoryId
    ) AS Owners;
END;
