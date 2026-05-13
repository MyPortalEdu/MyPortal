-- ============================================================================
-- Add the System.BulletinSettings permission, used to gate the bulletin
-- settings page (category taxonomy + the student-group audience allowlist).
-- The bulletin create/edit form continues to read categories under the
-- existing School.ViewSchoolBulletins permission; only the mutating endpoints
-- move to this new admin-tier permission.
-- ============================================================================

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'System.BulletinSettings', N'Bulletin Settings', N'System')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
