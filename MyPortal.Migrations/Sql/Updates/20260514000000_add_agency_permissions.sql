-- ============================================================================
-- Add permissions for viewing and editing agencies.
-- ============================================================================

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Agencies.ViewAgencies', N'View Agencies', N'Agencies'),
    (N'Agencies.EditAgencies', N'Edit Agencies', N'Agencies')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
