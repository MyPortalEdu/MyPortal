-- Adds the View permission for academic years so reads can be granted
-- independently of edits (every staff member needs to see term dates;
-- only admins should be editing).

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Curriculum.ViewAcademicYears', N'View Academic Years', N'Curriculum')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
