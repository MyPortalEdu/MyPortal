-- Student-profile permission catalogue (Student.{Verb}Student{Area}). Held by staff (UserType
-- defaults to 1 = Staff). Flat, all-pupils: access is gated by data-sensitivity area + role, NOT by
-- any viewer→student relationship — matches SIMS, where a teacher profile sees the whole pupil
-- population. See docs/student-profile-access.md. Default role grants are seeded separately.

MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    -- Basic details (identity, contact methods, addresses)
    (N'Student.ViewStudentBasicDetails',   N'View basic details',      N'Student.BasicDetails'),
    (N'Student.EditStudentBasicDetails',   N'Edit basic details',      N'Student.BasicDetails'),
    -- Registration (enrolment/boarder status, UPN/ULN, admission, year/reg/house)
    (N'Student.ViewStudentRegistration',   N'View registration',       N'Student.Registration'),
    (N'Student.EditStudentRegistration',   N'Edit registration',       N'Student.Registration'),
    -- Family & contacts
    (N'Student.ViewStudentFamily',         N'View family & contacts',  N'Student.Family'),
    (N'Student.EditStudentFamily',         N'Edit family & contacts',  N'Student.Family'),
    -- Cultural / ethnic (special-category)
    (N'Student.ViewStudentCultural',       N'View cultural & ethnic',  N'Student.Cultural'),
    (N'Student.EditStudentCultural',       N'Edit cultural & ethnic',  N'Student.Cultural'),
    -- Medical (health data)
    (N'Student.ViewStudentMedical',        N'View medical',            N'Student.Medical'),
    (N'Student.EditStudentMedical',        N'Edit medical',            N'Student.Medical'),
    -- Welfare / safeguarding (sharply gated)
    (N'Student.ViewStudentWelfare',        N'View welfare & safeguarding', N'Student.Welfare'),
    (N'Student.EditStudentWelfare',        N'Edit welfare & safeguarding', N'Student.Welfare'),
    -- SEN
    (N'Student.ViewStudentSen',            N'View SEN',                N'Student.Sen'),
    (N'Student.EditStudentSen',            N'Edit SEN',                N'Student.Sen'),
    -- Funding (FSM, top-up, pupil premium)
    (N'Student.ViewStudentFunding',        N'View funding',            N'Student.Funding'),
    (N'Student.EditStudentFunding',        N'Edit funding',            N'Student.Funding'),
    -- School history (previous schools, destination)
    (N'Student.ViewStudentSchoolHistory',  N'View school history',     N'Student.SchoolHistory'),
    (N'Student.EditStudentSchoolHistory',  N'Edit school history',     N'Student.SchoolHistory'),
    -- Documents
    (N'Student.ViewStudentDocuments',      N'View documents',          N'Student.Documents'),
    (N'Student.EditStudentDocuments',      N'Edit documents',          N'Student.Documents')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
                    [Area] = Source.[Area]

    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
