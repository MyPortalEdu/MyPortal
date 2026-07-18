-- ============================================================================
-- Remove the per-staff "Edit Staff Timetable" permission. Editing a timetable is
-- a whole-school scheduling action (Timetable.EditTimetables) — never done per
-- staff member — so the staff-profile timetable is view-only and this edit
-- permission was vestigial.
--
-- RolePermissions has no FK to Permissions, so clear any references first, then
-- delete the permission row. Idempotent.
-- ============================================================================

DELETE RP
FROM dbo.RolePermissions RP
JOIN dbo.Permissions P ON P.Id = RP.PermissionId
WHERE P.Name = N'Staff.EditAllStaffTimetable';
GO

DELETE FROM dbo.Permissions
WHERE Name = N'Staff.EditAllStaffTimetable';
GO
