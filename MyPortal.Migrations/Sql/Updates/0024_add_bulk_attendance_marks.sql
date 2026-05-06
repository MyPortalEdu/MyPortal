-- ============================================================================
-- Bulk-edit attendance marks support:
--   * Attendance.EditAttendanceMarksBulk permission (reception/admin can edit
--     any cell in a (StudentGroup, date range) grid, beyond the single-period
--     scope of the regular register flow).
--   * BulkAttendanceMarkList TVP — input shape for sp_attendance_marks_submit_bulk.
--     AttendanceCodeId is nullable: NULL signals "delete the existing mark for
--     this (Student, Week, Period) cell" so corrections can clear an entry.
-- ============================================================================

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Attendance.EditAttendanceMarksBulk', N'Edit Attendance Marks (Bulk)', N'Attendance.Marks')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);

GO

IF TYPE_ID(N'[dbo].[BulkAttendanceMarkList]') IS NULL
BEGIN
    CREATE TYPE [dbo].[BulkAttendanceMarkList] AS TABLE
    (
        [StudentId]          UNIQUEIDENTIFIER NOT NULL,
        [AttendanceWeekId]   UNIQUEIDENTIFIER NOT NULL,
        [AttendancePeriodId] UNIQUEIDENTIFIER NOT NULL,
        [AttendanceCodeId]   UNIQUEIDENTIFIER NULL,
        [Comments]           NVARCHAR(256)    NULL,
        [MinutesLate]        INT              NULL,
        PRIMARY KEY CLUSTERED ([StudentId], [AttendanceWeekId], [AttendancePeriodId])
    );
END;
