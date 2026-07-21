-- ============================================================================
-- Seed a starting student-profile permission set for the default staff roles,
-- mapped from the SIMS 7 permissions matrix onto MyPortal's roles. See the grant
-- table in docs/student-profile-access.md. IsDefault roles are protected from
-- delete/rename but their grants stay editable, so this is only a starting point.
--
-- Insert-only (NOT EXISTS on every row), so re-application never re-adds a grant a
-- school has since removed. Welfare is the sharply gated area — only SENCo / SLT /
-- Office see it, and only SENCo / Office edit it (mirrors SIMS, where class
-- teachers and tutors get neither). Bulk record *edit* lives on Office / Registrar
-- and SENCo.
--
-- Role ids are the well-known GUIDs from 20260717000100 / 20260721000100.
-- ============================================================================

INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    -- Teacher (011): view the record broadly; edit SEN (SENCo-led, teachers contribute). No Welfare.
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.EditStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000011', N'Student.ViewStudentDocuments'),

    -- Teaching Assistant (013): view core record; no Funding, no Welfare, no edit.
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000013', N'Student.ViewStudentDocuments'),

    -- Form Tutor (012): view the whole record bar Welfare.
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000012', N'Student.ViewStudentDocuments'),

    -- Head of Year (014): pastoral lead — view all bar Welfare; edit registration + medical.
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.EditStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.EditStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000014', N'Student.ViewStudentDocuments'),

    -- SENCo (015): full view + edit across every area, including Welfare.
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentWelfare'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentWelfare'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.ViewStudentDocuments'),
    (N'5EED0001-0000-4000-8000-000000000015', N'Student.EditStudentDocuments'),

    -- Senior Leadership Team (010): whole-record view, including Welfare. No edit.
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentWelfare'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-000000000010', N'Student.ViewStudentDocuments'),

    -- Office / Registrar (01E): full view + edit across every area (the record-maintenance role).
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentBasicDetails'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentRegistration'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentRegistration'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentFamily'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentFamily'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentCultural'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentCultural'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentMedical'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentMedical'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentWelfare'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentWelfare'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentSen'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentSen'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentFunding'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentFunding'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentSchoolHistory'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.ViewStudentDocuments'),
    (N'5EED0001-0000-4000-8000-00000000001E', N'Student.EditStudentDocuments')
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
