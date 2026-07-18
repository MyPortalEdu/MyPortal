-- ============================================================================
-- Seed the default roles. Runs after 20251101000000_add_identity (Name /
-- NormalizedName / ConcurrencyStamp) and 20260717000000 (UserType / IsDefault).
--
-- ConcurrencyStamp is seeded NON-NULL (reusing the row's Id as a stable stamp):
-- SqlRoleStore.UpdateAsync filters WHERE ConcurrencyStamp = @old, so a NULL stamp
-- would make the first edit of any seeded role fail the concurrency predicate.
--
-- NormalizedName = UPPER(Name); role names are ASCII, so this matches Identity's
-- ToUpperInvariant() normalisation (how AuthSeeder / SqlUserStore resolve roles).
--
-- Student / Parent carry fixed well-known GUIDs (referenced by
-- MyPortal.Common.Constants.SystemRoles for auto-assignment). The staff roles'
-- GUIDs are fixed only for idempotency — nothing references them in code.
--
-- Idempotent: SysAdmin is name-guarded (see below); the rest are insert-only
-- MERGE by Id, so a re-run never overwrites a school's edited Description/grants.
-- ============================================================================

-- SysAdmin: creation unified here, but guarded on NAME — an upgraded DB already
-- has an AuthSeeder-created row (random GUID); guarding on the well-known Id would
-- insert a SECOND 'System Administrator' and break AuthSeeder's single-row lookup.
-- Nothing references SysAdmin by GUID, so keeping the old row's id is harmless.
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = N'SYSTEM ADMINISTRATOR')
BEGIN
    INSERT INTO dbo.Roles (Id, Name, NormalizedName, Description, IsSystem, IsDefault, UserType, ConcurrencyStamp)
    VALUES (N'5EED0001-0000-4000-8000-000000000000', N'System Administrator', N'SYSTEM ADMINISTRATOR',
            N'Full, unrestricted access. Granted every permission on boot; cannot be edited or deleted.',
            1, 0, 1, N'5EED0001-0000-4000-8000-000000000000');
END;
GO

-- Default portal + staff roles. IsDefault = 1 (identity protected, grants editable).
-- Seeded with NO permissions — grants are added feature by feature.
MERGE INTO dbo.Roles AS Target
USING (VALUES
    (N'5EED0001-0000-4000-8000-000000000001', N'Student',                N'Portal access for pupils. Auto-assigned to student users.',                 2),
    (N'5EED0001-0000-4000-8000-000000000002', N'Parent',                 N'Portal access for parents and carers. Auto-assigned to parent users.',      3),
    (N'5EED0001-0000-4000-8000-000000000010', N'Senior Leadership Team', N'School leadership. Broad cross-area access.',                               1),
    (N'5EED0001-0000-4000-8000-000000000011', N'Teacher',                N'Classroom teacher.',                                                        1),
    (N'5EED0001-0000-4000-8000-000000000012', N'Form Tutor',             N'Registration / pastoral tutor for a form group.',                           1),
    (N'5EED0001-0000-4000-8000-000000000013', N'Teaching Assistant',     N'Classroom support staff.',                                                  1),
    (N'5EED0001-0000-4000-8000-000000000014', N'Head of Year',           N'Year-group pastoral lead.',                                                 1),
    (N'5EED0001-0000-4000-8000-000000000015', N'SENCo',                  N'Special educational needs coordinator.',                                    1),
    (N'5EED0001-0000-4000-8000-000000000016', N'Attendance Manager',     N'Manages attendance registers and codes.',                                   1),
    (N'5EED0001-0000-4000-8000-000000000017', N'Cover Manager',          N'Arranges cover for absent staff.',                                          1),
    (N'5EED0001-0000-4000-8000-000000000018', N'Exams Officer',          N'Manages examinations and entries.',                                         1),
    (N'5EED0001-0000-4000-8000-000000000019', N'Assessment Lead',        N'Manages assessment, marksheets and results.',                               1),
    (N'5EED0001-0000-4000-8000-00000000001A', N'Timetabler',             N'Builds and maintains the timetable.',                                       1),
    (N'5EED0001-0000-4000-8000-00000000001B', N'Admissions Officer',     N'Manages applications and admissions.',                                      1),
    (N'5EED0001-0000-4000-8000-00000000001C', N'HR Officer',             N'Manages staff records and HR processes.',                                   1),
    (N'5EED0001-0000-4000-8000-00000000001D', N'Finance Officer',        N'Manages fees, billing and finance.',                                        1)
) AS Source (Id, Name, Description, UserType)
ON Target.Id = Source.Id
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Name, NormalizedName, Description, IsSystem, IsDefault, UserType, ConcurrencyStamp)
    VALUES (Source.Id, Source.Name, UPPER(Source.Name), Source.Description, 0, 1, Source.UserType,
            CONVERT(NVARCHAR(36), Source.Id));
GO
