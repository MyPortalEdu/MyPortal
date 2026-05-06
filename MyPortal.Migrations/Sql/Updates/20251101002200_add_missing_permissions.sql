-- ============================================================================
-- Add permissions that exist as constants in MyPortal.Auth.Constants.Permissions
-- but were never seeded into dbo.Permissions:
--
--   * Attendance.ViewAttendanceSetup / EditAttendanceSetup — referenced by the
--     attendance setup UI but missing from 0012_add_attendance_mark_permissions.
--   * Attendance.UseRestrictedCodes — gate for using AttendanceCodes flagged
--     IsRestricted = 1 when submitting a register.
--   * Curriculum.EditAcademicYears — required by AcademicYearService.CreateAcademicYear.
--
-- MERGE-by-Name keeps this migration safely re-runnable: existing rows get
-- their FriendlyName / Area refreshed if drifted, missing rows are inserted.
-- ============================================================================

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Attendance.ViewAttendanceSetup', N'View Attendance Setup', N'Attendance.Setup'),
    (N'Attendance.EditAttendanceSetup', N'Edit Attendance Setup', N'Attendance.Setup'),
    (N'Attendance.UseRestrictedCodes',  N'Use Restricted Attendance Codes', N'Attendance.Marks'),
    (N'Curriculum.EditAcademicYears',   N'Edit Academic Years', N'Curriculum')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
