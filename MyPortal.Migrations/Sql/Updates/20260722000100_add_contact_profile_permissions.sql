-- Contact-profile permission catalogue + starting role grants. Held by staff (UserType defaults to
-- 1 = Staff). A contact record is family/guardian information; view is broad (anyone who sees the
-- pupil record), edit is record-maintenance (office + SENCo), mirroring Student.Family. See
-- docs/student-profile-access.md.

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    (N'Contact.ViewContactDetails', N'View contact details', N'Contact.Details'),
    (N'Contact.EditContactDetails', N'Edit contact details', N'Contact.Details')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName], [Area] = Source.[Area]

    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
GO

-- Starting grants: view for the roles that see the pupil record; edit for Office/Registrar + SENCo.
INSERT INTO dbo.RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), CAST(g.RoleId AS UNIQUEIDENTIFIER), p.Id
FROM (VALUES
    (N'5EED0001-0000-4000-8000-000000000010', N'Contact.ViewContactDetails'), -- SLT
    (N'5EED0001-0000-4000-8000-000000000011', N'Contact.ViewContactDetails'), -- Teacher
    (N'5EED0001-0000-4000-8000-000000000012', N'Contact.ViewContactDetails'), -- Form Tutor
    (N'5EED0001-0000-4000-8000-000000000013', N'Contact.ViewContactDetails'), -- Teaching Assistant
    (N'5EED0001-0000-4000-8000-000000000014', N'Contact.ViewContactDetails'), -- Head of Year
    (N'5EED0001-0000-4000-8000-000000000015', N'Contact.ViewContactDetails'), -- SENCo
    (N'5EED0001-0000-4000-8000-000000000015', N'Contact.EditContactDetails'), -- SENCo
    (N'5EED0001-0000-4000-8000-00000000001E', N'Contact.ViewContactDetails'), -- Office / Registrar
    (N'5EED0001-0000-4000-8000-00000000001E', N'Contact.EditContactDetails')  -- Office / Registrar
) AS g(RoleId, PermName)
JOIN dbo.Permissions p ON p.Name = g.PermName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = CAST(g.RoleId AS UNIQUEIDENTIFIER) AND rp.PermissionId = p.Id);
GO
