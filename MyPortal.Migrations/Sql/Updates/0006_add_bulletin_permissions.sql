MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    -- System
    (N'School.ViewSchoolBulletins', N'View School Bulletins', N'School.Bulletins'),
    (N'School.EditSchoolBulletins', N'Edit School Bulletins', N'School.Bulletins'),
    (N'School.ApproveSchoolBulletins', N'Approve School Bulletins', N'School.Bulletins')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
