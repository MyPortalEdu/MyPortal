MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Timetable.ViewTimetables', N'View Timetables', N'Timetable'),
    (N'Timetable.EditTimetables', N'Edit Timetables', N'Timetable')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
