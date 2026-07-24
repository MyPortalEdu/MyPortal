-- Grant the Emergency Contacts (next of kin) permissions to the same roles that hold the other
-- HR/safeguarding All-scope areas: the HR/Personnel role gets View + Edit; the Senior Leadership
-- Team role gets View. Mirrors how Pre-Employment Checks was granted in the default-role seed.
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    -- HR / Personnel — full maintenance.
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.ViewAllStaffEmergencyContacts'),
    (N'5EED0001-0000-4000-8000-00000000001C', N'Staff.EditAllStaffEmergencyContacts'),
    -- Senior Leadership Team — view only.
    (N'5EED0001-0000-4000-8000-000000000010', N'Staff.ViewAllStaffEmergencyContacts')
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
