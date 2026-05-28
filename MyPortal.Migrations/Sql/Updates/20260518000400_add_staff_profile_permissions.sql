-- ============================================================================
-- Staff-profile permission catalogue (scoped: Own / Managed / All).
--
-- Backs the relationship-aware staff details page — see
-- docs/staff-profile-access.md. Naming: Staff.{Verb}{Scope}Staff{Section}. Only
-- the cells that exist here are grantable; the resolver can never grant a string
-- that isn't seeded. None are auto-assigned to a role — assignment is a separate
-- admin step.
--
-- Supersedes the unscoped placeholders Staff.ViewStaffBasicDetails /
-- Staff.EditStaffBasicDetails (never shipped). The DELETEs below clean those up
-- on any database that ran the earlier draft.
-- ============================================================================

DELETE rp
FROM dbo.RolePermissions rp
    INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id
WHERE p.[Name] IN (N'Staff.ViewStaffBasicDetails', N'Staff.EditStaffBasicDetails');

DELETE FROM dbo.Permissions
WHERE [Name] IN (N'Staff.ViewStaffBasicDetails', N'Staff.EditStaffBasicDetails');

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    -- Basic details
    (N'Staff.ViewOwnStaffBasicDetails',            N'View basic details (own)',            N'Staff.BasicDetails'),
    (N'Staff.ViewManagedStaffBasicDetails',        N'View basic details (managed)',        N'Staff.BasicDetails'),
    (N'Staff.ViewAllStaffBasicDetails',            N'View basic details (all)',            N'Staff.BasicDetails'),
    (N'Staff.EditManagedStaffBasicDetails',        N'Edit basic details (managed)',        N'Staff.BasicDetails'),
    (N'Staff.EditAllStaffBasicDetails',            N'Edit basic details (all)',            N'Staff.BasicDetails'),
    -- Equality & identity (special-category)
    (N'Staff.ViewOwnStaffEqualityDetails',         N'View equality & identity (own)',      N'Staff.Equality'),
    (N'Staff.ViewAllStaffEqualityDetails',         N'View equality & identity (all)',      N'Staff.Equality'),
    (N'Staff.EditAllStaffEqualityDetails',         N'Edit equality & identity (all)',      N'Staff.Equality'),
    -- Contact methods
    (N'Staff.ViewOwnStaffContactMethods',          N'View contact methods (own)',          N'Staff.Contact'),
    (N'Staff.ViewManagedStaffContactMethods',      N'View contact methods (managed)',      N'Staff.Contact'),
    (N'Staff.ViewAllStaffContactMethods',          N'View contact methods (all)',          N'Staff.Contact'),
    (N'Staff.EditOwnStaffContactMethods',          N'Edit contact methods (own)',          N'Staff.Contact'),
    (N'Staff.EditManagedStaffContactMethods',      N'Edit contact methods (managed)',      N'Staff.Contact'),
    (N'Staff.EditAllStaffContactMethods',          N'Edit contact methods (all)',          N'Staff.Contact'),
    -- Addresses
    (N'Staff.ViewOwnStaffAddresses',               N'View addresses (own)',                N'Staff.Addresses'),
    (N'Staff.ViewManagedStaffAddresses',           N'View addresses (managed)',            N'Staff.Addresses'),
    (N'Staff.ViewAllStaffAddresses',               N'View addresses (all)',                N'Staff.Addresses'),
    (N'Staff.EditOwnStaffAddresses',               N'Edit addresses (own)',                N'Staff.Addresses'),
    (N'Staff.EditManagedStaffAddresses',           N'Edit addresses (managed)',            N'Staff.Addresses'),
    (N'Staff.EditAllStaffAddresses',               N'Edit addresses (all)',                N'Staff.Addresses'),
    -- Emergency contacts
    (N'Staff.ViewOwnStaffEmergencyContacts',       N'View emergency contacts (own)',       N'Staff.EmergencyContacts'),
    (N'Staff.ViewManagedStaffEmergencyContacts',   N'View emergency contacts (managed)',   N'Staff.EmergencyContacts'),
    (N'Staff.ViewAllStaffEmergencyContacts',       N'View emergency contacts (all)',       N'Staff.EmergencyContacts'),
    (N'Staff.EditOwnStaffEmergencyContacts',       N'Edit emergency contacts (own)',       N'Staff.EmergencyContacts'),
    (N'Staff.EditManagedStaffEmergencyContacts',   N'Edit emergency contacts (managed)',   N'Staff.EmergencyContacts'),
    (N'Staff.EditAllStaffEmergencyContacts',       N'Edit emergency contacts (all)',       N'Staff.EmergencyContacts'),
    -- Professional
    (N'Staff.ViewOwnStaffProfessionalDetails',     N'View professional details (own)',     N'Staff.Professional'),
    (N'Staff.ViewManagedStaffProfessionalDetails', N'View professional details (managed)', N'Staff.Professional'),
    (N'Staff.ViewAllStaffProfessionalDetails',     N'View professional details (all)',     N'Staff.Professional'),
    (N'Staff.EditManagedStaffProfessionalDetails', N'Edit professional details (managed)', N'Staff.Professional'),
    (N'Staff.EditAllStaffProfessionalDetails',     N'Edit professional details (all)',     N'Staff.Professional'),
    -- Qualifications & CPD
    (N'Staff.ViewOwnStaffQualifications',          N'View qualifications & CPD (own)',     N'Staff.Qualifications'),
    (N'Staff.ViewManagedStaffQualifications',      N'View qualifications & CPD (managed)', N'Staff.Qualifications'),
    (N'Staff.ViewAllStaffQualifications',          N'View qualifications & CPD (all)',     N'Staff.Qualifications'),
    (N'Staff.EditOwnStaffQualifications',          N'Edit qualifications & CPD (own)',     N'Staff.Qualifications'),
    (N'Staff.EditManagedStaffQualifications',      N'Edit qualifications & CPD (managed)', N'Staff.Qualifications'),
    (N'Staff.EditAllStaffQualifications',          N'Edit qualifications & CPD (all)',     N'Staff.Qualifications'),
    -- Employment & contract (incl. salary, bank)
    (N'Staff.ViewOwnStaffEmploymentDetails',       N'View employment & contract (own)',    N'Staff.Employment'),
    (N'Staff.ViewAllStaffEmploymentDetails',       N'View employment & contract (all)',    N'Staff.Employment'),
    (N'Staff.EditAllStaffEmploymentDetails',       N'Edit employment & contract (all)',    N'Staff.Employment'),
    -- Pre-employment checks (DBS, right to work)
    (N'Staff.ViewAllStaffPreEmploymentChecks',     N'View pre-employment checks (all)',    N'Staff.PreEmploymentChecks'),
    (N'Staff.EditAllStaffPreEmploymentChecks',     N'Edit pre-employment checks (all)',    N'Staff.PreEmploymentChecks'),
    -- Absences & leave (health data)
    (N'Staff.ViewOwnStaffAbsences',                N'View absences & leave (own)',         N'Staff.Absences'),
    (N'Staff.ViewManagedStaffAbsences',            N'View absences & leave (managed)',     N'Staff.Absences'),
    (N'Staff.ViewAllStaffAbsences',                N'View absences & leave (all)',         N'Staff.Absences'),
    (N'Staff.EditManagedStaffAbsences',            N'Edit absences & leave (managed)',     N'Staff.Absences'),
    (N'Staff.EditAllStaffAbsences',                N'Edit absences & leave (all)',         N'Staff.Absences'),
    -- Timetable & teaching load
    (N'Staff.ViewOwnStaffTimetable',               N'View timetable (own)',                N'Staff.Timetable'),
    (N'Staff.ViewManagedStaffTimetable',           N'View timetable (managed)',            N'Staff.Timetable'),
    (N'Staff.ViewAllStaffTimetable',               N'View timetable (all)',                N'Staff.Timetable'),
    (N'Staff.EditAllStaffTimetable',               N'Edit timetable (all)',                N'Staff.Timetable'),
    -- Documents
    (N'Staff.ViewOwnStaffDocuments',               N'View documents (own)',                N'Staff.Documents'),
    (N'Staff.ViewManagedStaffDocuments',           N'View documents (managed)',            N'Staff.Documents'),
    (N'Staff.ViewAllStaffDocuments',               N'View documents (all)',                N'Staff.Documents'),
    (N'Staff.EditOwnStaffDocuments',               N'Edit documents (own)',                N'Staff.Documents'),
    (N'Staff.EditManagedStaffDocuments',           N'Edit documents (managed)',            N'Staff.Documents'),
    (N'Staff.EditAllStaffDocuments',               N'Edit documents (all)',                N'Staff.Documents'),
    -- Performance / appraisal
    (N'Staff.ViewManagedStaffPerformanceDetails',  N'View performance (managed)',          N'Staff.Performance'),
    (N'Staff.ViewAllStaffPerformanceDetails',      N'View performance (all)',              N'Staff.Performance'),
    (N'Staff.EditManagedStaffPerformanceDetails',  N'Edit performance (managed)',          N'Staff.Performance'),
    (N'Staff.EditAllStaffPerformanceDetails',      N'Edit performance (all)',              N'Staff.Performance')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
                    [Area] = Source.[Area]

    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
