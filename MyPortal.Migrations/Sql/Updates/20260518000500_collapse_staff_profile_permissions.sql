-- Collapse staff-profile permissions: ContactMethods / Addresses / EmergencyContacts → folded
-- into BasicDetails; Qualifications → folded into ProfessionalDetails. The DELETEs are no-ops
-- on a fresh DB and clean up any DB that ran 20260518000400. RolePermissions FK cleared first.
-- See docs/staff-profile-access.md.

DECLARE @removed TABLE ([Name] NVARCHAR(256) PRIMARY KEY);

INSERT INTO @removed ([Name]) VALUES
    -- Contact methods
    (N'Staff.ViewOwnStaffContactMethods'),
    (N'Staff.ViewManagedStaffContactMethods'),
    (N'Staff.ViewAllStaffContactMethods'),
    (N'Staff.EditOwnStaffContactMethods'),
    (N'Staff.EditManagedStaffContactMethods'),
    (N'Staff.EditAllStaffContactMethods'),
    -- Addresses
    (N'Staff.ViewOwnStaffAddresses'),
    (N'Staff.ViewManagedStaffAddresses'),
    (N'Staff.ViewAllStaffAddresses'),
    (N'Staff.EditOwnStaffAddresses'),
    (N'Staff.EditManagedStaffAddresses'),
    (N'Staff.EditAllStaffAddresses'),
    -- Emergency contacts
    (N'Staff.ViewOwnStaffEmergencyContacts'),
    (N'Staff.ViewManagedStaffEmergencyContacts'),
    (N'Staff.ViewAllStaffEmergencyContacts'),
    (N'Staff.EditOwnStaffEmergencyContacts'),
    (N'Staff.EditManagedStaffEmergencyContacts'),
    (N'Staff.EditAllStaffEmergencyContacts'),
    -- Qualifications & CPD
    (N'Staff.ViewOwnStaffQualifications'),
    (N'Staff.ViewManagedStaffQualifications'),
    (N'Staff.ViewAllStaffQualifications'),
    (N'Staff.EditOwnStaffQualifications'),
    (N'Staff.EditManagedStaffQualifications'),
    (N'Staff.EditAllStaffQualifications');

DELETE rp
FROM dbo.RolePermissions rp
    INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id
    INNER JOIN @removed r        ON r.[Name] = p.[Name];

DELETE p
FROM dbo.Permissions p
    INNER JOIN @removed r ON r.[Name] = p.[Name];
