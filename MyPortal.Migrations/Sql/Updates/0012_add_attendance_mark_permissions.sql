MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Attendance.ViewAttendanceMarks', N'View Attendance Marks', N'Attendance.Marks'),
    (N'Attendance.EditAttendanceMarks', N'Edit Attendance Marks', N'Attendance.Marks')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
