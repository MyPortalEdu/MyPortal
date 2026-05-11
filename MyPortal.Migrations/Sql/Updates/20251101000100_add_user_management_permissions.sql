MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    -- System
    (N'System.ViewUsers',               N'View Users',                N'System.Users'),
    (N'System.EditUsers',               N'Edit Users',                N'System.Users'),
    (N'System.ViewRoles',              N'View Roles',               N'System.Users'),
    (N'System.EditRoles',              N'Edit Roles',               N'System.Users')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
