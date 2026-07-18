-- ============================================================================
-- Seed a sensible starting permission set for the default (IsDefault) staff
-- roles, inspired by the equivalent SIMS roles' responsibilities mapped onto
-- MyPortal's current permissions. IsDefault roles are protected from delete /
-- rename but their grants stay editable, so this is only a starting point —
-- schools tune it in the role editor.
--
-- Insert-only (NOT EXISTS on every row), so re-application never re-adds a grant
-- a school has since removed. System Administrator is untouched (AuthSeeder
-- grants it every permission on boot). Student / Parent get nothing: every
-- permission today is Staff-audience.
--
-- Role ids are the well-known GUIDs from 20260717000100_seed_default_roles.sql.
-- ============================================================================

-- 1) Self-service baseline for EVERY default staff role: see your own profile,
--    timetable, absences, documents (+ edit own documents) and school bulletins.
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.IsDefault = 1
  AND r.UserType = 1
  AND p.Name IN (
      N'Staff.ViewOwnStaffBasicDetails',
      N'Staff.ViewOwnStaffProfessionalDetails',
      N'Staff.ViewOwnStaffEmploymentDetails',
      N'Staff.ViewOwnStaffEqualityDetails',
      N'Staff.ViewOwnStaffAbsences',
      N'Staff.ViewOwnStaffTimetable',
      N'Staff.ViewOwnStaffDocuments',
      N'Staff.EditOwnStaffDocuments',
      N'School.ViewSchoolBulletins')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO

-- 2) Role-specific grants (role GUID, permission Name).
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    -- Senior Leadership Team — school-wide view + bulletins + appraisals.
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffTimetable'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffAbsences'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffProfessionalDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffEmploymentDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffPerformanceDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffPreEmploymentChecks'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.EditAllStaffPerformanceDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Attendance.ViewAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-000000000010', N'School.ViewPastoralStructure'),
    (N'5EED0001-0000-4000-8000-000000000010', N'School.EditSchoolBulletins'),
    (N'5EED0001-0000-4000-8000-000000000010', N'School.PinSchoolBulletins'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Timetable.ViewTimetables'),

    -- Teacher — registers, staff directory, colleague timetables.
    (N'5EED0001-0000-4000-8000-000000000011', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Attendance.EditAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Staff.ViewAllStaffTimetable'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Timetable.ViewTimetables'),

    -- Form Tutor — Teacher + pastoral view + bulletins.
    (N'5EED0001-0000-4000-8000-000000000012', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Attendance.EditAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Attendance.ViewAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Staff.ViewAllStaffTimetable'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-000000000012', N'School.ViewPastoralStructure'),
    (N'5EED0001-0000-4000-8000-000000000012', N'School.EditSchoolBulletins'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Timetable.ViewTimetables'),

    -- Teaching Assistant — view registers, directory, timetable.
    (N'5EED0001-0000-4000-8000-000000000013', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Timetable.ViewTimetables'),

    -- Head of Year — attendance, pastoral, managed-staff view + appraisal.
    (N'5EED0001-0000-4000-8000-000000000014', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Attendance.EditAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Attendance.ViewAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-000000000014', N'School.ViewPastoralStructure'),
    (N'5EED0001-0000-4000-8000-000000000014', N'School.EditPastoralStructure'),
    (N'5EED0001-0000-4000-8000-000000000014', N'School.EditSchoolBulletins'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Staff.ViewManagedStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Staff.ViewManagedStaffTimetable'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Staff.ViewManagedStaffAbsences'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Staff.ViewManagedStaffPerformanceDetails'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Staff.EditManagedStaffPerformanceDetails'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Timetable.ViewTimetables'),

    -- SENCo — directory, pastoral, attendance, curriculum (view).
    (N'5EED0001-0000-4000-8000-000000000015', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000015', N'School.ViewPastoralStructure'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Curriculum.ViewAcademicYears'),

    -- Attendance Manager — full attendance.
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.EditAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.EditAttendanceMarksBulk'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.UseRestrictedCodes'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.ViewAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance.EditAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-000000000016', N'Staff.ViewAllStaffTimetable'),

    -- Cover Manager — all-staff timetable + absences + directory.
    (N'5EED0001-0000-4000-8000-000000000017', N'Staff.ViewAllStaffTimetable'),
    (N'5EED0001-0000-4000-8000-000000000017', N'Staff.ViewAllStaffAbsences'),
    (N'5EED0001-0000-4000-8000-000000000017', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000017', N'Attendance.ViewAttendanceMarks'),
    (N'5EED0001-0000-4000-8000-000000000017', N'Timetable.ViewTimetables'),

    -- Exams Officer — curriculum + directory (view).
    (N'5EED0001-0000-4000-8000-000000000018', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-000000000018', N'Staff.ViewAllStaffBasicDetails'),

    -- Assessment Lead — curriculum + directory (view).
    (N'5EED0001-0000-4000-8000-000000000019', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-000000000019', N'Staff.ViewAllStaffBasicDetails'),

    -- Timetabler — whole-school timetable + AY structure + attendance setup.
    (N'5EED0001-0000-4000-8000-00000000001A', N'Timetable.ViewTimetables'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Timetable.EditTimetables'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Curriculum.ViewAcademicYears'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Curriculum.EditAcademicYears'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Attendance.ViewAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Attendance.EditAttendanceSetup'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Staff.ViewAllStaffTimetable'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Staff.ViewAllStaffAbsences'),
    (N'5EED0001-0000-4000-8000-00000000001A', N'School.ViewPastoralStructure'),

    -- Admissions Officer — agencies + directory (view).
    (N'5EED0001-0000-4000-8000-00000000001B', N'Agencies.ViewAgencies'),
    (N'5EED0001-0000-4000-8000-00000000001B', N'Staff.ViewAllStaffBasicDetails'),

    -- HR Officer — full view+edit of all staff records.
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffBasicDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffProfessionalDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffProfessionalDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffEmploymentDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffEmploymentDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffEqualityDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffEqualityDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffAbsences'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffAbsences'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffPreEmploymentChecks'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffPreEmploymentChecks'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffDocuments'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffDocuments'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffPerformanceDetails'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffTimetable'),

    -- Finance Officer — agencies + staff pay (view).
    (N'5EED0001-0000-4000-8000-00000000001D', N'Agencies.ViewAgencies'),
    (N'5EED0001-0000-4000-8000-00000000001D', N'Agencies.EditAgencies'),
    (N'5EED0001-0000-4000-8000-00000000001D', N'Staff.ViewAllStaffEmploymentDetails')
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
