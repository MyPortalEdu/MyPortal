﻿/*

Script used to seed the MyPortal database with initial basedata.

Author: R. Richards

*/

--- MODIFY BASEDATA ---

BEGIN TRANSACTION;

EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

MERGE INTO [dbo].[SystemSettings] AS Target
USING (VALUES
           ('iBillPaymntPeriodLength', '6'),
           ('gSchoolLogoId', null),
           ('iDbVersion', '1000')
)
    AS Source(Name, Setting)
ON Target.Name = Source.Name

WHEN NOT MATCHED THEN
    INSERT (Name, Setting)
    VALUES (Name, Setting);

MERGE INTO [dbo].[Roles] AS Target
USING (VALUES

           ('719FAA21-AA56-4924-9DB6-3B1344E1E1A6','admast', 'ADMAST', 'Administration Assistant', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1A7','admoff', 'ADMOFF', 'Admissions Officer', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1A8','asscoo', 'ASSCOO', 'Assessment Coordinator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1A9','assopr', 'ASSOPR', 'Assessment Operator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AA','attman', 'ATTMAN', 'Attendance Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AB','attopr', 'ATTOPR', 'Attendance Operator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AC','superu', 'SUPERU', 'Super User', 0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AD','clstch', 'CLSTCH', 'Class Teacher', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AE','curman', 'CURMAN', 'Curricular Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1AF','covman', 'COVMAN', 'Cover Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B1','exmofr', 'EXMOFR', 'Exams Officer', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B2','exmopr', 'EXMOPR', 'Exams Operator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B3','pasman', 'PASMAN', 'Pastoral Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B4','procoo', 'PROCOO', 'Profiles Coordinator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B5','proopr', 'PROOPR', 'Profiles Operator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B6','regtut', 'REGTUT', 'Registrations Tutor', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B7','retman', 'RETMAN', 'Returns Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B8','retopr', 'RETOPR', 'Returns Operator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1B9','schadm', 'SCHADM', 'School Administrator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BA','sencoo', 'SENCOO', 'SEN Coordinator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BB','senmgt', 'SENMGT', 'Senior Management Team', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BC','sysman', 'SYSMAN', 'System Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BD','tmtblr', 'TMTBLR', 'Timetabler', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BE','studnt', 'STUDNT', 'Student', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1BF','parent', 'PARENT', 'Parent', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C1','tchast', 'TCHAST', 'Teaching Assistant', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C2','safgld', 'SAFGLD', 'Safeguarding Lead', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C3','suptch', 'SUPTCH', 'Supply Teacher', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C4','perofr', 'PEROFR', 'Personnel Officer', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C5','perast', 'PERAST', 'Personnel Assistant', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C6','peeofr', 'PEEOFR', 'Personnel Events Officer', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C7','perman', 'PERMAN', 'SP Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C8','linmgr', 'LINMGR', 'SP Line Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1C9','apprsr', 'APPRSR', 'SP Appraiser', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1CA','perstf', 'PERSTF', 'SP Staff', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1CB','peradm', 'PERADM', 'SP Administrator', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1CC','feeman', 'FEEMAN', 'Fees Manager', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1CD','feeclk', 'FEECLK', 'Fees Clerk', 0x0000000000000000000000000000000000),
           ('719FAA21-AA56-4924-9DB6-3B1344E1E1CE','recclk', 'RECCLK', 'Receipts Clerk', 0x0000000000000000000000000000000000)
)
    AS Source (Id, Name, NormalizedName, Description, Permissions)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Name = Source.Name
WHEN NOT MATCHED THEN
    INSERT (Id, Name, NormalizedName, Description, Permissions, System)
    VALUES (Id, Name, NormalizedName, Description, Permissions, 1);

MERGE INTO [dbo].[Users] AS Target
USING (VALUES
           ('0E22EE12-2269-4297-F666-08D861890F99', 'system', 'SYSTEM', 0, 'AQAAAAEAACcQAAAAECpmszIMBeEAuZU6NPzlW2pVODCv+5K93wUxBvCg406S1PnMH5kqK6Ij4H4yIfcXng==', '7TSHWVR5LEO5O4BBDK7GXTT7I3BVYNRX', '51418c4d-c883-4bdc-a5f4-6ae30be0edb3', 0, 0, 1, 0, GETDATE(), 0, 1)
)
    AS Source (Id, UserName, NormalizedUserName, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedDate, UserType, Enabled)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, UserName, NormalizedUserName, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedDate, UserType, Enabled)
    VALUES (Id, UserName, NormalizedUserName, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedDate, UserType, Enabled);

MERGE INTO [dbo].[UserRoles] AS Target
USING (VALUES
           ('0E22EE12-2269-4297-F666-08D861890F99', '719FAA21-AA56-4924-9DB6-3B1344E1E1AC')
)
    AS Source (UserId, RoleId)
ON Target.UserId = Source.UserId AND Target.RoleId = Source.RoleId

WHEN NOT MATCHED THEN
    INSERT (UserId, RoleId)
    VALUES (UserId, RoleId);

MERGE INTO [dbo].[AspectTypes] AS Target
USING (VALUES
           ('84F43913-ED25-4839-B130-62AC605DEBFA', 'Grade'),
           ('84F43913-ED25-4839-B130-62AC605DEBFB', 'Mark-Decimal'),
           ('84F43913-ED25-4839-B130-62AC605DEBFC', 'Mark-Integer'),
           ('84F43913-ED25-4839-B130-62AC605DEBFD', 'Comment')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1)

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description;

MERGE INTO [dbo].[GradeSets] AS Target
USING (VALUES
           ('11A467CD-FEC7-4A28-85FD-086F2B8982BD', 'TP NC Levels', 'National Curriculum Levels', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'JC A Result', 'General Certificate of Education / Free Standing Mathematics Qualifications / AQA Level 3 Certificate in (award)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'JC B Result', 'General Certificate of Education (lower case) / Vocational Certificate of Education (lower case) /  AQA Level 3 Certificate in (unit)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'JC C Result', 'General Certificate of Education (Double Award)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'JC D Result', 'General National Vocational Qualification / OCR Nationals / Applied Science Pathways / BTEC Technicals', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'JC E Result', 'General National Vocational Qualification (lower case)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'JC F Result', 'General Certificate of Secondary Education', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'JC G Result', 'Certificate of Achievement / Entry Level', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'JC H Result', 'Advanced Extension Award', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'JC I Result', 'General Certificate of Education (including grade ‘N’)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'JC J Result', 'General Certificate of Education (including grade ‘N’) (lower case)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'JC K Result', 'General Certificate of Education (Dual Award)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'JC L Result', 'Key Skills (Level) / Business Enterprise / Essential Skills / Occupational Studies', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'JC M Result', 'Key Skills / Functional Skills / OCR Progression (Pass/Fail)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CB', 'JC N Result', 'GNVQ Language Units / Functional Skills / GOML / ACETS', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'JC O Result', 'STEP', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'JC P Result', 'General Certificate of Secondary Education including grade p (lower case)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'JC Q Result', 'General Certificate of Secondary Education (lower case)', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982CF', 'JC R Result', 'VRQ', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'JC S Result', 'BTEC triple grades', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'JC T Result', 'BTEC double grades', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'JC U Result', 'DiDA', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D3', 'JC V Result', 'Level 1/Level 2', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'JC W Result', 'Asset Languages – Breakthrough', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'JC X Result', 'Asset Languages – Preliminary', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'JC Y Result', 'Asset Languages – Intermediate', 1, 1),
           ('11A467CD-FEC7-4A28-85FD-086F2B8982D7', 'JC Z Result', 'Asset Languages – Advanced', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982D8', 'JC 1 Result', 'Asset Languages – Proficiency', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982D9', 'JC 2 Result', 'Asset Languages – Mastery', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'JC 3 Result', 'BTEC Firsts with Distinction*/ Applied Science Pathways', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'JC 4 Result', 'GCE A level, Extended Project and PL3', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'JC 5 Result', 'GCE A level, Extended Project and PL3 only (lower case)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'JC 6 Result', 'PL1 and Project L1', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'JC 7 Result', 'PL1 and Project L1 (lower case)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'JC 8 Result', 'PL2 and Project L2 / CiDA and DiDA', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'JC 9 Result', 'PL2 and Project L2 (lower case) / CiDA and DiDA', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'JC 10 Result', 'GCE A level double award', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'JC 11 Result', 'GCE A level with additional AS level', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'JC 12 Result', 'Pre-U Level 3 Certificates', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'JC 13 Result', 'ESOL for work', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'JC 14 Result', 'Global Perspectives', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'JC 15 Result', 'GCSE double award', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'JC 16 Result', 'AICE Diploma / OCR Cambridge Level 2 / OCR Cambridge Level 3', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'JC 17 Result', 'Diploma in Pre-U Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'JC 18 Result', 'BTEC Level 3 Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'JC 19 Result', 'BTEC Level 3 Extended Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'JC 20 Result', 'AQA Certificate in Further Mathematics', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'JC 21 Result', 'Cambridge Nationals / CCEA Occupational Studies / BTEC Technical Awards', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'JC 22 Result', 'Cambridge Nationals (lower case)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'JC 23 Result', 'OCR Cambridge Level 2 / OCR Cambridge Level 3', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'JC 24 Result', 'OCR Cambridge Level 3', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'JC 25 Result', 'OCR Cambridge Level 3 Extended Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'JC 26 Result', 'Edexcel Primary Curriculum', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'JC 27 Result', 'Edexcel Lower Secondary Curriculum', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'JC 28 Result', 'BTEC Firsts', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'JC 29 Result', 'BTEC Firsts (double award)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'JC 30 Result', 'BTEC Firsts (external units)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F6', 'JC 31 Result', 'General Certificate of Secondary Education (English Speaking & Listening unit)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'JC 32 Result', 'Welsh Bacc Advanced', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'JC 33 Result', 'Cambridge Technical Certificate/ Extended Certificate', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'JC 34 Result', 'Cambridge Technical Foundation Diploma/ Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'JC 35 Result', 'Cambridge Technical Extended Diploma', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'JC 36 Result', 'National/Foundation - Skills Challenge Certificate', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'JC 37 Result', 'Foundation (post-16) - Skills Challenge Certificate', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'JC 38 Result', 'National/Foundation Welsh Baccalaureate', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'JC 39 Result', 'National and National/Foundation - Skills Challenge Unit', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'JC 40 Result', 'GCSE (grades 9 to 1)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898300', 'JC 41 Result', 'GCSE (grades 9-9 to 1-1)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898301', 'JC 42 Result', 'GCSE (endorsement)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898302', 'JC 43 Result', 'GCE (endorsement)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898303', 'JC 44 Result', 'GCSE (with grade C*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898304', 'JC 45 Result', 'GCSE (lower case with C*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898305', 'JC 46 Result', 'Year 10 Assessments', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898306', 'JC 47 Result', 'Foundation (Post-16) - Skills Challenge Unit', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898307', 'JC 48 Result', 'GCSE (higher tier grades 9 to 3)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898308', 'JC 49 Result', 'GCSE (foundation tier grades 5 to 1)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898309', 'JC 50 Result', 'GCSE (higher tier grades 9-9 to 4-3)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830A', 'JC 51 Result', 'GCSE (foundation tier grades 5-5 to 1-1)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830B', 'JC 52 Result', 'GCSE (higher tier grades A* to D)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830C', 'JC 53 Result', 'GCSE (foundation tier grades C to G)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830D', 'JC 54 Result', 'GCSE unit (higher tier grades a* to d)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830E', 'JC 55 Result', 'GCSE unit (foundation tier grades c to g)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89830F', 'JC 56 Result', 'GCSE (higher tier grades A*A* to CD)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898310', 'JC 57 Result', 'GCSE (foundation tier grades CC to GG)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898311', 'JC 58 Result', 'Applied General Units', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898312', 'JC 59 Result', 'Applied General Awards', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898313', 'JC 60 Result', 'Vocational Units (End Point Assessments)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898314', 'JC 61 Result', 'GCSE (lower case with c*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898315', 'JC 62 Result', 'GCSE double award (with grade C*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898316', 'JC 63 Result', 'GCSE (higher tier lower case with c*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898317', 'JC 64 Result', 'GCSE (foundation tier lower case with c*)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898318', 'JC 65 Result', 'Oxford AQA International GCSE Double Award', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898319', 'JC 66 Result', 'Oxford AQA International GCSE Endorsement', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831A', 'JC 67 Result', 'AQA Level 1/2 Award and Technical Award – Certification', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831B', 'JC 68 Result', 'AQA Level 1/2 Award and Technical Award - Unit', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831C', 'JC 69 Result', 'Pearson LCCI', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831D', 'JC 70 Result', 'OCR Technical', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831E', 'JC 71 Result', 'BTEC Technical Internal Unit', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B89831F', 'JC 72 Result', 'BTEC Technical External Unit', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898320', 'JC 73 Result', 'BTEC Technical Qualification', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898321', 'JC 74 Result', 'BTEC Nationals (External Units)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898322', 'JC 75 Result', 'OCR Nationals (External Units)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898323', 'JC 76 Result', 'BTEC Entry 3 (grades P to M)', 1, 1),
           ('11A469CD-FEC7-4A28-85FD-086F2B898324', 'JC 77 Result', 'BTEC Entry 3 (grades PP to MM)', 1, 1)
)
    AS Source (Id, Name, Description, Active, System)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Active = Source.Active

WHEN NOT MATCHED THEN
    INSERT (Id, Name, Description, Active, System)
    VALUES (Id, Name, Description, Active, System);

MERGE INTO [dbo].[Grades] AS Target
USING (VALUES
           ('7F14DB02-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '1c', '1 low', 1),
           ('7F14DB03-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '1b', '1 secure', 2),
           ('7F14DB04-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '1a', '1 high', 3),
           ('7F14DB05-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '2c', '2 low', 4),
           ('7F14DB06-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '2b', '2 secure', 5),
           ('7F14DB07-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '2a', '2 high', 6),
           ('7F14DB08-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '3c', '3 low', 7),
           ('7F14DB09-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '3b', '3 secure', 8),
           ('7F14DB0A-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '3a', '3 high', 9),
           ('7F14DB0B-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '4c', '4 low', 10),
           ('7F14DB0C-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '4b', '4 secure', 11),
           ('7F14DB0D-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '4a', '4 high', 12),
           ('7F14DB0E-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '5c', '5 low', 13),
           ('7F14DB0F-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '5b', '5 secure', 14),
           ('7F14DB10-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '5a', '5 high', 15),
           ('7F14DB11-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '6c', '6 low', 16),
           ('7F14DB12-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '6b', '6 secure', 17),
           ('7F14DB13-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '6a', '6 high', 18),
           ('7F14DB14-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '7c', '7 low', 19),
           ('7F14DB15-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '7b', '7 secure', 20),
           ('7F14DB16-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '7a', '7 high', 21),
           ('7F14DB17-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '8c', '8 low', 22),
           ('7F14DB18-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '8b', '8 secure', 23),
           ('7F14DB19-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', '8a', '8 high', 24),
           ('7F14DB1A-DF20-4D8C-B8A3-17D9D46A02EE', '11A467CD-FEC7-4A28-85FD-086F2B8982BD', 'E', 'Exceptional Performance', 25),
           ('dddb1953-264e-4822-9749-91e4911dd35a', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', '13', 'Grade 13', 1),
           ('013c5505-fa65-4ea6-a2c0-03699140612b', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', '14', 'Grade 14', 0),
           ('30c5b9ff-3318-424a-84c5-a45e02bc12c9', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', '15', 'Grade 15', 0),
           ('2b13e40c-8c41-4959-91b6-d04c6754d529', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', 'Q', 'Pending', 0),
           ('1a98f972-6a19-4dd7-81f7-7821a5352695', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', 'U', 'Unclassified', 0),
           ('5c7ffbaa-dcb0-48fa-9bc1-b2c34a3fede7', '11A469CD-FEC7-4A28-85FD-086F2B8982D8', 'X', 'No Result', 0),
           ('50dd6173-d620-47c0-bbfc-b9647ac8e30e', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', '**', 'Grade A*A*', 280),
           ('afc3a289-6480-44de-aa9f-780711420bb8', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', '*A', 'Grade A*A', 263.33),
           ('14e336e6-946a-4c84-af24-352a44a025ed', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'AA', 'Grade AA', 246.67),
           ('d0dbcf66-8fe9-4c09-8a94-13dbd88ae572', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'AB', 'Grade AB', 230),
           ('08d4136a-84b2-438a-a357-ff6443dda4d5', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'BB', 'Grade BB', 213.33),
           ('8db22122-6d2c-4fb0-9473-771fb19d77c5', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'BC', 'Grade BC', 196.67),
           ('bc17a63c-1256-41dd-aa60-4d938db63cec', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'CC', 'Grade CC', 180),
           ('5df0004b-82d6-4bf1-ad35-164ca6a4b5a9', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'CD', 'Grade CD', 163.33),
           ('5946efef-b888-4fba-9442-4f82334bd374', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'DD', 'Grade DD', 146.67),
           ('d8115e8e-a255-4669-9c62-56b2edcbbdd8', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'DE', 'Grade DE', 130),
           ('2992e84b-ed7b-42f3-9089-f0926336cd45', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'EE', 'Grade EE', 113.33),
           ('c2200f3a-75e1-42a2-836d-d2047c2e5075', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'Q', 'Pending', 0),
           ('1ef04574-ecc6-4615-82f9-71ad999df859', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'U', 'Unclassified', 0),
           ('b3cab0e6-cb47-4619-96b4-b780d732aae4', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'X', 'No Result', 0),
           ('0acf3c80-0912-4a69-ac14-c377021f443a', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', '*A', 'Grade A*A', 199.17),
           ('d3eb0d44-4048-49af-9593-4beb176eecfe', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'AA', 'Grade AA', 185),
           ('218359a9-b5d1-4700-9637-7470fd696b2c', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'AB', 'Grade AB', 172.5),
           ('9603b7ef-6a21-4513-97ea-41cb873d8069', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'BB', 'Grade BB', 160),
           ('f09a99c3-1524-4422-a5e6-3e8ff405ad4e', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'BC', 'Grade BC', 147.5),
           ('0d97e153-9d7b-4a4d-b419-34dddfc7244f', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'CC', 'Grade CC', 135),
           ('f9710532-d370-4565-932b-1e983c62e35e', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'CD', 'Grade CD', 122.5),
           ('f091009d-d258-4884-ad22-6a4b4f31c21a', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'DD', 'Grade DD', 110),
           ('a2ad17d5-6117-47bc-bf12-a555fbb49d43', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'DE', 'Grade DE', 97.5),
           ('9defbdfb-711e-48b0-ab28-6c6ca0d8a3b1', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'EE', 'Grade EE', 85),
           ('a6ba4f56-4f92-49cd-b50a-208a42a54cc0', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'Q', 'Pending', 0),
           ('5f240dc7-af89-4fc5-a0f7-77dcc2354d1a', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'U', 'Unclassified', 0),
           ('f01546c7-9765-4db3-b6c9-c6a53bff7862', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'X', 'No Result', 0),
           ('6a8b43fe-b7c9-4097-b861-0c4cefd9d3ce', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'D1', 'Grade D1', 60),
           ('1b386862-06dd-4db6-9659-b7c877f2c3dd', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'D2', 'Grade D2', 54.17),
           ('a0d4cc41-c5a8-42d8-9b95-88f29374e03b', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'D3', 'Grade D3', 48.33),
           ('9ff7d4ef-c807-4f69-9f5c-bb2c3e0e64bb', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'M1', 'Grade M1', 42.5),
           ('dc476ba7-c47b-4895-9d97-bfa11e0cd61d', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'M2', 'Grade M2', 36.67),
           ('3693083b-54cc-4733-b909-517d7c252d54', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'M3', 'Grade M3', 30),
           ('1b0ec24d-7028-4824-8e2e-94b6a6195796', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'P1', 'Grade P1', 23.33),
           ('5e7d0f2d-b934-4bd3-b923-b711b7b32438', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'P2', 'Grade P2', 16.67),
           ('d8f81be6-850d-4f1a-b396-56e34953e50e', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'P3', 'Grade P3', 10),
           ('ca09d989-0b7c-4644-adbf-32b4378ae97c', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'Q', 'Pending', 0),
           ('1cbc702d-b424-41d8-bd79-f846a4dd89ca', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'U', 'Unclassified', 0),
           ('c8e4d613-8178-4802-975f-b850a02153a9', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'X', 'No Result', 0),
           ('25bbbb45-c1c2-451a-a1a4-201ff5435c64', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'E3', 'Grade E3', 0),
           ('2de6c2d2-1f02-4a64-8249-743cfd0cb499', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'L1', 'Grade L1', 0),
           ('ae047427-b58d-4ef3-a955-a96897eecd18', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'Q', 'Pending', 0),
           ('a889461b-0a4a-4c04-b80d-71c8e8d1d9d3', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'U', 'Unclassified', 0),
           ('06a37c9d-236a-4cad-8b11-9e13adaffa5e', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'X', 'No Result', 0),
           ('54180beb-20b1-4fd0-9bd7-690b52bc4bd4', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'D1', 'Grade D1', 0),
           ('e5b6408e-3c4c-4761-8302-bd79bf254690', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'D2', 'Grade D2', 0),
           ('af331519-bccf-45be-9b3b-446724cd8225', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'D3', 'Grade D3', 0),
           ('84845658-da51-4d4e-8808-4454d92c150a', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'M1', 'Grade M1', 0),
           ('f16976c3-2c91-4132-84ac-325f05b849d6', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'M2', 'Grade M2', 0),
           ('f05e0b07-3f0e-4655-bbc8-1e1e7db149ee', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'M3', 'Grade M3', 0),
           ('f5e374c9-75e9-4023-86c5-b8ca4b9cf9db', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'P1', 'Grade P1', 0),
           ('59e25a64-31dc-4515-8454-5b040ba3b305', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'P2', 'Grade P2', 0),
           ('2937f23a-52f9-4e8b-b460-a34957a82018', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'P3', 'Grade P3', 0),
           ('875cf6e1-1480-430f-9807-ae1090e75e1f', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'Q', 'Pending', 0),
           ('843f07ee-e8d3-4f12-8ea0-224c1023261f', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'U', 'Unclassified', 0),
           ('5b1dee75-8ee5-4122-bf25-5cc5ad8602d5', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'W', 'Grade W', 0),
           ('54cdccb1-02fc-4c8f-89f6-5fff3dc8e969', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'X', 'No Result', 0),
           ('1ef6fa6b-a0fc-49ea-af89-508eacd89f58', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', '**', 'Grade A*A*', 44.17),
           ('11c862b3-f456-4506-8465-06c6f83237e6', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', '*A', 'Grade A*A', 41.75),
           ('42f06d9c-9dd9-4e2f-8791-6bf7854de5f0', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'AA', 'Grade AA', 39.33),
           ('730f33b7-f148-4063-ac3c-44ac12314aae', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'AB', 'Grade AB', 36.92),
           ('f96d886e-184a-41e7-b1dc-6cd7e64601f9', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'BB', 'Grade BB', 34.5),
           ('bf830013-8804-4758-a665-39b37e863a21', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'BC', 'Grade BC', 32.08),
           ('f3d35ac8-dda9-4d17-b83d-384943f2dff1', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'CC', 'Grade CC', 29.67),
           ('d64e14d6-bb26-463a-aa6e-d0cac0ab86bd', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'CD', 'Grade CD', 27.33),
           ('fc4286d8-e495-49f6-96ca-ea4e58b1b6c4', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'DD', 'Grade DD', 25),
           ('5bf0f739-b1e3-4d93-94c2-8c659b987b04', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'DE', 'Grade DE', 22.67),
           ('6e46860c-1fa0-465d-9175-43d95255e4c7', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'EE', 'Grade EE', 20.33),
           ('89f61f0d-c2e0-4a9f-a7bb-c08fc81a7b96', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'EF', 'Grade EF', 18.08),
           ('050a4132-0425-4f0e-b622-b71166e31b74', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'FF', 'Grade FF', 15.83),
           ('b05e2f56-7580-4f4f-af9e-af49cb170488', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'FG', 'Grade FG', 13.58),
           ('f7b99d7e-0cdb-4623-8480-97ebec556ac2', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'GG', 'Grade GG', 11.33),
           ('d5fde175-40d2-4cf8-838d-45f61ca6b3ee', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'Q', 'Pending', 0),
           ('b04e41de-09ed-43c2-ace2-b3533abe1198', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'U', 'Unclassified', 0),
           ('71335cae-44b7-4705-8880-2b15553a3dfd', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'X', 'No Result', 0),
           ('bed41d83-b2ed-402a-bcb3-f8f5b5362ba3', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'D', 'Distinction', 0),
           ('4724db6f-230e-4953-acc7-4fa5ffb821ee', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'F', 'Fail', 0),
           ('8ef72ff9-3de5-433f-8fde-5787ebfe6f70', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'M', 'Merit', 0),
           ('cc3e8257-3f2f-4f2b-be99-ebb82d3152b0', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'P', 'Pass', 0),
           ('1000f231-6484-49e4-8208-7b8e904c3a8c', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'Q', 'Pending', 0),
           ('3214e1f8-96b5-4c8a-abd8-ad2fbd3d9bb0', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'U', 'Unclassified', 0),
           ('3ad37da7-3432-4a1e-a525-4d473b4fecbd', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'X', 'No Result', 0),
           ('5e9b1e43-99eb-46db-a834-dc7612134f02', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'N', 'No Award', 0),
           ('17fb3d53-96fb-47e0-8126-633e5da42915', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'P', 'Pass', 0),
           ('f7447aa7-6878-4de1-9434-d38a6937df3f', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'Q', 'Pending', 0),
           ('c867ad24-576f-4a4a-86a5-adeb7b9cff88', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'U', 'Unclassified', 0),
           ('d846075e-f23a-4a16-9d45-d70b2151b957', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'X', 'No Result', 0),
           ('c8ec4d4d-ccbd-4616-9ad9-6efcea921dc4', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'D1', 'Grade D*D*', 660),
           ('2f4e02d2-53d7-4f77-b19b-f8be478708e8', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'D2', 'Grade D*D', 600),
           ('84c2c58f-321f-4ce5-bb10-8b5755f17ce0', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'DD', 'Grade DD', 540),
           ('b62a08e7-5ac1-457c-839e-8e60624335c8', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'DM', 'Grade DM', 480),
           ('699c5b0f-540c-4a2b-8b35-ee63840e1c71', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'MM', 'Grade MM', 420),
           ('fa784695-687d-456f-b11f-04aec29dad43', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'MP', 'Grade MP', 360),
           ('8e72814e-5ec9-4037-894b-ff9ea79f830c', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'PP', 'Grade PP', 300),
           ('7fd8d4ff-8d2c-422f-89fc-d95bf8a052cd', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'Q', 'Pending', 0),
           ('e944b8bb-3d84-427c-9c06-908c62d687a3', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'U', 'Unclassified', 0),
           ('0051fc4e-da8b-4a60-bcf7-e770a14f6412', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'X', 'No Result', 0),
           ('73dec631-f66a-4904-aa1f-5f0cc3f5d783', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D1', 'Grade D*D*D*', 967.5),
           ('b596f3aa-14de-486c-8902-78ba9281f90c', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D2', 'Grade D*D*D', 915),
           ('10f0b475-2371-4a7d-9a04-f89db629a30a', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D3', 'Grade D*DD', 862.5),
           ('02176746-9b20-4358-920f-c213a66368bc', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D4', 'Grade DDD', 810),
           ('de69d8e5-1537-46c5-99a5-ed2de8c0726b', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D5', 'Grade DDM', 757.5),
           ('0daff964-bc58-4f35-b6b5-dbd715e12a26', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'D6', 'Grade DMM', 705),
           ('a98c0a88-1b46-4fbe-ba6d-109f7e588d1e', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'M1', 'Grade MMM', 652.5),
           ('36807242-7f72-4ca7-b91d-4cbbdeac852e', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'M2', 'Grade MMP', 600),
           ('d8d92717-734e-44a2-bec4-40aa58f32fb8', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'M3', 'Grade MPP', 547.5),
           ('39f6e92d-f792-4247-8739-6f0f52794bbc', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'P1', 'Grade PPP', 495),
           ('cbca166c-8ff4-4e33-acc7-fcbbb1702918', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'Q', 'Pending', 0),
           ('2dd01342-6a98-431e-8363-06c98db5505f', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'U', 'Unclassified', 0),
           ('58c1b71a-b03e-48ee-93d6-4f00d12d16ff', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'X', 'No Result', 0),
           ('2978d815-aa53-44d2-8f04-b0352b43c9d5', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', '16', 'Grade 16', 1),
           ('b629a334-8146-4222-825f-2f1fa6c2b6bf', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', '17', 'Grade 17', 0),
           ('74254b60-738b-4a81-9ff0-12c7ccf1180e', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', '18', 'Grade 18', 0),
           ('6a6b9e17-0c2c-4a4a-b4d1-ffbf7a663a88', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', 'Q', 'Pending', 0),
           ('30e60fff-f967-48ab-a4f4-3be402bb8502', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', 'U', 'Unclassified', 0),
           ('3f727ae1-4fbc-479d-9d55-e740c6a2f2d4', '11A469CD-FEC7-4A28-85FD-086F2B8982D9', 'X', 'No Result', 0),
           ('af5be960-669e-4a65-8d3b-5fc83221f789', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'A', 'Grade A', 52),
           ('f03342b2-004e-4445-98dd-763e6e525dc7', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'A*', 'Grade A*', 58),
           ('ca65b842-5d84-424c-bfaa-cf6ede8d74a7', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'A^', 'Grade A^', 58),
           ('6aae21d3-7639-473b-9c81-d012fcf2b502', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'B', 'Grade B', 46),
           ('70632c1c-4b89-4328-bb6f-816b5d5702b7', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'C', 'Grade C', 40),
           ('bb133b37-2119-426a-8353-561e8e8a5d65', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'Q', 'Pending', 0),
           ('6bb23c9b-8edf-42d8-a7d5-7460557c06f9', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'U', 'Unclassified', 0),
           ('078142eb-660a-4d8f-8c80-d7e972418055', '11A469CD-FEC7-4A28-85FD-086F2B8982EB', 'X', 'No Result', 0),
           ('7c1f6394-726f-4a4a-b5e1-a522cc6081f9', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', '*2', 'Distinction* L2', 24.83),
           ('d894bcb8-1e9b-475d-890a-76255742a1ba', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'D1', 'Distinction L1', 13.67),
           ('532bae56-63b4-4ec1-aa23-cb6ecd771a48', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'D2', 'Distinction L2', 22),
           ('4bfe2803-b97f-43ba-9d1a-356e02ed5d90', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'M1', 'Merit L1', 11),
           ('43b0bbce-14f2-42ea-ba01-50936881f1e5', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'M2', 'MeritL2', 19.17),
           ('1696c4dd-71c7-468e-9777-d8ab89b378b6', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'P1', 'Pass L1', 7.25),
           ('b01d60db-4a57-4c63-a079-bb5fa6dbeda0', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'P2', 'Pass L2', 16.33),
           ('e1456389-77d6-4c76-8477-9aaa33f0b3ab', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'Q', 'Pending', 0),
           ('70daa343-051e-483b-b01e-0b68badb4a9d', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'U', 'Unclassified', 0),
           ('15ede079-5eac-4eea-9175-d8cfbf835b11', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'X', 'No Result', 0),
           ('77f3741d-261c-490d-9b12-90d36c7bddfe', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', '*2', 'distinction* L2', 58),
           ('9c108d17-acf8-4f7f-bc3e-476f9d02fbb8', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'd1', 'distinction L1', 34),
           ('82e0399d-6463-4f5e-b747-483d09d7c956', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'd2', 'distinction L2', 52),
           ('de549d9c-989b-46e9-bca4-10e52e3dec08', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'm1', 'merit L1', 28),
           ('43d0658a-56cd-4385-bbad-c2d3da99394e', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'm2', 'meritL2', 46),
           ('05b33cc4-fe01-45be-8689-f50afab65293', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'p1', 'pass L1', 19),
           ('683e4e2a-4dd4-448b-8e10-90f9c8a52f9c', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'p2', 'pass L2', 40),
           ('f3fa2a3e-4f74-442b-8a59-da337f7d6c16', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'Q', 'Pending', 0),
           ('30036cc3-a3f2-4d19-b35e-7e3e94ed010c', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'u', 'unclassified', 0),
           ('dc392e60-c634-4ec8-a88f-9ccf25c21752', '11A469CD-FEC7-4A28-85FD-086F2B8982ED', 'X', 'No Result', 0),
           ('c1333c72-6d8a-4084-80fa-52f65b0deee1', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'D', 'Distinction', 64),
           ('d1812025-beff-4a02-848c-ae6355432638', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'D*', 'Distinction*', 64),
           ('bb725697-b12b-4aba-8a68-255a8b7195ab', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'M', 'Merit L', 0),
           ('ffda2653-1681-4ca5-97b6-efc09af11cf0', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'P', 'Pass L', 0),
           ('fd82b091-0c29-49a6-9d71-11153f539baf', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'Q', 'Pending', 0),
           ('798a5844-58d4-400d-b83b-808fdac93624', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'X', 'No Result', 0),
           ('7ca41802-7121-49b0-8c0e-612d7c69da43', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'D1', 'D*D*', 71.33),
           ('9436ec51-e904-4d33-9059-60508e2b1108', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'D2', 'D*D', 63.83),
           ('5b888427-15e3-4e5d-a7e1-51f2a6f8748b', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'DD', 'DD', 56.33),
           ('5017d9e2-6e74-4d73-9f9f-e9029cdef10f', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'DM', 'DM', 51.33),
           ('ab3e9314-c442-4ec0-bf18-a3b66dc6be92', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'MM', 'MM', 46.33),
           ('7fe2a1e9-a122-4404-a86f-6cffd6e58088', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'MP', 'MP', 41.33),
           ('085ec2cd-2daf-4ee4-b5ad-6dd9b22fd178', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'PP', 'PP', 36.33),
           ('931983b3-65b0-403a-bd1b-4ca2c69412da', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'Q', 'Pending', 0),
           ('558e9a75-e25e-4d7f-8cdb-31de1ea3aff1', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'X', 'No Result', 0),
           ('15c0c6d4-2f58-4a73-967e-f7cca3662088', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D1', 'D*D*D*', 121.33),
           ('e620c6a4-d6dc-4b9f-a8ff-d929e161b0fd', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D2', 'D*D*D', 111.33),
           ('162fd885-c053-4d98-b92d-b51c4def615b', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D3', 'D*DD', 80),
           ('bb5c8a27-a171-4a8b-859c-65765eccb8f3', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D4', 'DDD', 70),
           ('d0878bf6-9de8-40b7-80e5-ff4ddcd78960', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D5', 'DDM', 63.33),
           ('33fc0a0f-e35e-48e0-9c58-968629e1efa4', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'D6', 'DMM', 56.67),
           ('036385ca-99a0-49c8-9128-bc661272cf8e', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'M1', 'MMM', 50),
           ('3e178d16-7289-45c5-8b49-4cc6d24d6195', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'M2', 'MMP', 43.33),
           ('23b68e83-c811-4f05-b468-058d31a764b5', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'M3', 'MPP', 36.67),
           ('4fe8fe1c-de96-4845-b1a7-c291c1f230fb', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'P1', 'PPP', 30),
           ('c8d3cdc4-6454-4fa8-b15f-5547af29fedf', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'Q', 'Pending', 0),
           ('af313b4e-c9ed-4f70-bf78-87cfb64302b9', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'X', 'No Result', 0),
           ('55f3779f-9d5b-4eb4-9ff6-df7d287aa9ea', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'P1', 'Primary Level 1', 64),
           ('1b7370d3-5ebf-40ca-9fd0-ec717b5c81d0', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'P2', 'Primary Level 2', 58),
           ('d3892cbc-d4b7-4ad0-86bb-98d2ef73f816', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'P3', 'Primary Level 3', 52),
           ('111e96ac-fc2e-4b86-8f19-3016b46c6cbe', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'Q', 'Pending', 0),
           ('7113698d-2ae9-4545-b605-db28b88891a5', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'U', 'Unclassified', 0),
           ('290e5755-318e-4a08-ad1d-1152aaa96db1', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'X', 'No Result', 0),
           ('70601aaf-e7e2-4707-8496-70b7907ab495', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'Q', 'Pending', 0),
           ('758898c7-7b37-4786-9403-2c5aac07e8db', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'S1', 'Secondary Level 1', 64),
           ('6914bce2-18b9-4954-894c-48124114fe6d', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'S2', 'Secondary Level 2', 58),
           ('640e2f7c-871a-4238-afa2-476040759eb5', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'S3', 'Secondary Level 3', 52),
           ('e1c551e9-1cb2-4734-9e34-75b048912ab4', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'S4', 'Secondary Level 4', 52),
           ('9e20e850-be47-4b6f-9d8e-9d59b7af0b3d', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'U', 'Unclassified', 0),
           ('d25940af-6932-4dfb-8c45-6b4db856a53b', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'X', 'No Result', 0),
           ('d378cc8c-b207-48cd-9235-fcd02faecd94', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', '*2', 'Distinction* L2', 24.83),
           ('65e1f8a4-5a70-474d-bb92-b99deeeae9f5', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'D2', 'Distinction L2', 22),
           ('cb31e2fb-1060-457a-86a1-be275d4daa7c', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'M2', 'MeritL2', 19.17),
           ('e6cb64b9-4c07-4015-8db5-f3fe228e6c8d', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'P1', 'Pass L1', 9.75),
           ('200bdfbb-6379-47ff-900b-4b4e07da28c9', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'P2', 'Pass L2', 16.33),
           ('189a9285-84e9-4b32-be42-93d598d4b6b1', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'Q', 'Pending', 0),
           ('1f04962d-0cd5-4270-ba67-e98f8216fd34', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'U', 'Unclassified', 0),
           ('85d68c2f-21ef-4a11-986c-d9ac8f946d2d', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'X', 'No Result', 0),
           ('56d1da5c-2f2a-4cea-988c-b0175802a3cf', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'D1', 'D*D* L2', 5),
           ('17a2b9fc-3ea2-48ef-8cbc-3258136f3a64', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'D2', 'D*D L2', 4),
           ('02969626-603d-4958-82f7-1b27435aabff', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'DD', 'DD L2', 3),
           ('1b5e429d-53a0-4fea-a9f6-950a3abc8112', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'DM', 'DM L2', 2),
           ('7cffe0b6-f898-46ce-8d55-a7ce0e1fea43', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'MM', 'MM L2', 1),
           ('83e65df1-3714-4a29-8e0b-bd7ae18e2e30', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'MP', 'MP L2', 1),
           ('d69ccae8-2637-4c85-aa6d-24f5c6b0effb', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'P1', 'Pass L1', 0),
           ('fa1fbd3e-c24c-4c30-9a83-5b467b0699d1', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'PP', 'PP L2', 0),
           ('9662feaa-4e6e-4217-b942-69370da027df', '11A469CD-FEC7-4A28-85FD-086F2B8982F4', 'U', 'Unclassified', 0),
           ('4941e118-4c2e-48cc-92c8-ff239f7cb776', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'D', 'Distinction', 110),
           ('5ff3ed0b-906e-45e8-918a-0d4ba94321e7', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'D*', 'Distinction *', 116),
           ('d34ba883-e0b9-4c5b-ab0a-8b69c3c9f93f', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'M', 'Merit', 98),
           ('ad0578e8-4e71-4ef8-aeff-eb0ffd426a7f', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'P', 'Pass', 80),
           ('d99d9105-d2b1-43bb-999f-9f110f357351', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'Q', 'Pending', 0),
           ('5f4e08bc-2597-4d55-b635-c16c5b5af66d', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'U', 'Unclassified', 0),
           ('6a722e05-ac30-4c89-ade8-7838239d1569', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'X', 'No Result', 0),
           ('395b5368-9b00-425d-97f7-fbb264b60e31', '11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'D', 'Distinction L2', 5),
           ('72dac649-db57-490e-801b-32d40e79c23f', '11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'M', 'Merit L2', 4),
           ('f1f0f65d-864f-4a01-b887-e1f721db1dd5', '11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'P', 'Pass L2', 3),
           ('448e19ef-8c99-46ba-b114-e1f27508a0ce', '11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'P1', 'Pass L1', 2),
           ('13fb7943-2e17-4fb5-b82c-141fd5b0d7a4', '11A469CD-FEC7-4A28-85FD-086F2B8982F5', 'U', 'Unclassified', 0),
           ('c8e1f9e6-3424-4839-9953-67cca6fbf3be', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', '1', 'Grade 1', 1),
           ('5bef42c6-140e-429e-817b-2b4e563d65d1', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', '2', 'Grade 2', 2),
           ('5430ad70-f122-40bb-b8ac-e0ad53eedd37', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', '3', 'Grade 3', 3),
           ('38b95df3-c6c3-4d57-99fb-6a9dc2ec8be2', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', '4', 'Grade 4', 4),
           ('7384e6cd-3f73-4228-8b60-5db4680f7d33', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', '5', 'Grade 5', 5),
           ('eae9f71d-3464-4314-bd21-f1d546b64b6e', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', 'Q', 'Pending', 0),
           ('c7ea0645-2927-427e-b4da-948a891eb883', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', 'U', 'Unclassified', 0),
           ('1e1773e4-ec44-4723-b703-effeffb0c491', '11A469CD-FEC7-4A28-85FD-086F2B8982F6', 'X', 'No Result', 0),
           ('ee749248-f0b5-4aad-95e3-a753dcc65d74', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'A', 'Grade A', 4),
           ('91da7632-c67f-4488-a1ea-6b116b425c16', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'A*', 'Grade A*', 5),
           ('ba1881b4-95ad-4ab4-ab57-e73e51762de2', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'B', 'Grade B', 3),
           ('0626ea87-47f4-4507-8bcd-b2e68d81381b', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'C', 'Grade C', 2),
           ('5cbc33a0-16f3-470c-aba9-18e3a2fc1753', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'F', 'Fail', 1),
           ('40f38dca-20d4-49cd-93a4-84967db2a8aa', '11A469CD-FEC7-4A28-85FD-086F2B8982F7', 'U', 'Unclassified', 0),
           ('4539cd7f-f84e-4e2f-8ed7-7ef2ac73759b', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'D', 'Distinction', 3),
           ('2d07b70d-e735-4889-8b5a-4427639f831f', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'D*', 'Distinction*', 4),
           ('d4a26472-b9c0-4c13-895b-a005cd7a5640', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'M', 'Merit', 2),
           ('f97b6660-985c-40eb-83da-a53b4acfe0e4', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'P', 'Pass', 1),
           ('df1de534-38ed-41a0-8b83-2de3a279e020', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'Q', 'Result Pending', 0),
           ('d928b342-c14f-4c35-8007-6043bb7134ec', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'U', 'Unclassified', 0),
           ('b7cf3966-b24b-49f4-b40b-48fcbe4a1a63', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'X', 'No Result Awarded', 0),
           ('76103750-9d84-4d81-bb20-d5516641e1cd', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'D1', 'D*D*', 69),
           ('dfb75bb1-6ba0-4549-aaab-11f8004f9f51', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'D2', 'D*D', 58.67),
           ('08491d08-14e7-4883-acc2-50f8f26bbeb6', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'DD', 'DD', 48.33),
           ('01fdb5e2-3020-4463-9525-5d46bf464496', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'DM', 'DM', 41.33),
           ('354fd47c-c0ee-4dec-a7b1-a5044be15c3a', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'MM', 'MM', 34.33),
           ('5e903667-18b3-4483-8540-859e0877a274', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'MP', 'MP', 27.33),
           ('b0007211-d651-463b-8c98-dfbb43483861', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'PP', 'PP', 20.33),
           ('86d0752f-7377-4cb9-b008-dbcbb4ab1e37', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'Q', 'Result Pending', 0),
           ('fb9c4783-0a88-4f0f-b7a0-f94dd1ff2836', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'U', 'Unclassified', 0),
           ('bdcee55c-c559-49a3-8d52-2bfbfa999e24', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'X', 'No Result Awarded', 0),
           ('9a706424-cb9c-4c53-92e0-ea5b17316bf1', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D1', 'D*D*D*', 10),
           ('36077ffe-0a13-4356-bccf-944ae93c299a', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D2', 'D*D*D', 9),
           ('e9e634d7-f564-4f98-965e-6a8db16b3b6f', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D3', 'D*DD', 8),
           ('4d4fee3a-06ca-4aa0-86f8-6399fa2e4952', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D4', 'DDD', 7),
           ('d2321139-848f-4dea-a815-b402f884b607', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D5', 'DDM', 6),
           ('8c456161-4355-4c0d-ac86-311bae37fd43', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'D6', 'DMM', 5),
           ('9a35ee36-bceb-49ac-aed2-4355683bbd70', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'M1', 'MMM', 4),
           ('2b1a1aa8-b2ac-4a12-8532-83ba09d9c24b', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'M2', 'MMP', 3),
           ('a5676d7a-a884-4beb-afda-5141244eee2b', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'M3', 'MPP', 2),
           ('b61613d1-9f7b-4e7e-af09-7d9e5b365598', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'P1', 'PPP', 1),
           ('6515b452-24c7-4b2e-8d17-19a1b25fca9c', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'Q', 'Result Pending', 0),
           ('361f2d97-1e29-4024-8ead-15d31668323a', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'U', 'Unclassified', 0),
           ('188a3047-12b9-412f-8a85-4e7cf56d16a1', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'X', 'No Result Awarded', 0),
           ('2a5b102d-6e2b-4a90-ae13-ccad081672ed', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'A', 'Grade A National', 5),
           ('4ee6c3d5-c738-496b-9840-adb7487c9b5e', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'A*', 'Grade A* National', 6),
           ('feb7ce10-8483-4c6f-81f4-8a0e6a46b25b', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'B', 'Grade B National', 4),
           ('2775b6f2-d866-4a97-bc49-a7aedde1e12f', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'C', 'Grade C National', 3),
           ('0a5631ae-db95-42fa-8bb1-cbddc35dcfe0', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'P', 'Pass Foundation', 2),
           ('9ab24c34-dba3-471b-ad32-71ab744406f0', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'P*', 'Pass* Foundation', 1),
           ('9d1de48e-910a-4cd8-ace6-71d871abeb46', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'Q', 'Result Pending', 0),
           ('385e6eba-95d5-4793-829e-3fdc37206585', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'U', 'Unclassified', 0),
           ('1206856d-4787-4f25-bc9d-cd2f228fdf1c', '11A469CD-FEC7-4A28-85FD-086F2B8982FB', 'X', 'No Result Awarded', 0),
           ('68b5ee2c-cc58-4d7e-8471-1d94b862f880', '11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'P', 'Pass', 2),
           ('c01d7698-07a8-4607-8e7d-88d9ed1dd27a', '11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'P*', 'Pass*', 1),
           ('32e9dfa6-5c22-40f4-b444-2a4f6a790e3e', '11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'Q', 'Result Pending', 0),
           ('d2dd49a7-866d-41aa-aad2-b811fb23d8fd', '11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'U', 'Unclassified', 0),
           ('ab23a658-dec8-4234-b4ce-877cf2006113', '11A469CD-FEC7-4A28-85FD-086F2B8982FC', 'X', 'No Result Awarded', 0),
           ('1b631551-821d-455c-b3b8-3592adb7926d', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'F', 'Fail', 0),
           ('ac2a8a80-9759-4f62-999c-e459bf7f1c23', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'P1', 'Foundation Pass', 1),
           ('872eb275-f83c-4aba-aa9c-7d62cf3d2227', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'P2', 'National Pass', 2),
           ('f86e3f44-5c7e-4da2-aaae-af5280fd4b70', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'Q', 'Result Pending', 0),
           ('5a11a48c-e187-45b6-b672-57c7382df460', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'U', 'Unclassified', 0),
           ('80eef89a-0144-4219-9f57-c25ffefdf841', '11A469CD-FEC7-4A28-85FD-086F2B8982FD', 'X', 'No Result Awarded', 0),
           ('1f8c3936-ab91-400a-92d0-6f75bbd6f7d7', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'D2', 'Distinction L2', 4),
           ('1175b73b-69da-4317-9926-b184a4a979a5', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'M2', 'Merit L2', 3),
           ('430b167b-47b1-4252-ad41-a3cd0fa36e8f', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'P1', 'Pass L1', 1),
           ('90910b1b-a93b-4c35-848e-78b162f474e5', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'P2', 'Pass L2', 2),
           ('e981c547-045e-470a-aec3-a1b82b79776b', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'Q', 'Result Pending', 0),
           ('ffd5a6ff-b109-4c63-8ce2-5c84256f0401', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'U', 'Unclassified', 0),
           ('d70757b6-a058-438a-b1b6-1435e043befe', '11A469CD-FEC7-4A28-85FD-086F2B8982FE', 'X', 'No Result Awarded', 0),
           ('39832f84-c7a4-4693-be07-1b2ab2e21159', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'A', 'Grade A', 123.33),
           ('ec8bba55-b17d-41bf-af92-cf64fba7bc4f', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'A*', 'Grade A*', 140),
           ('da1838d9-5c0a-49a2-be75-2b6252232600', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'B', 'Grade B', 106.67),
           ('9b674535-7572-406b-bb29-ec7a13ee9ae6', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'C', 'Grade C', 90),
           ('8d41ccf8-0108-43b6-a187-ae525ea17d50', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'D', 'Grade D', 73.33),
           ('f2e715e4-1504-41d3-98d5-18c2d36d210e', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'E', 'Grade E', 56.67),
           ('941688f0-6e98-4106-bf37-cf8461131cd1', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'Q', 'Pending', 0),
           ('19793d1d-fb43-4dce-a815-d5c208571d2d', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'U', 'Unclassified', 0),
           ('87d834b4-2401-4e4e-aff1-8bd0aa00e08b', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'X', 'No Result', 0),
           ('04506af1-d9c6-46c2-a3fa-e3dda98964a9', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '1', 'Grade 1', 1),
           ('6dcf293d-a55c-4b9a-9be5-15604d0be95e', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '2', 'Grade 2', 2),
           ('37758a73-2a53-4eb6-b43b-3a690a245ef2', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '3', 'Grade 3', 3),
           ('282cc55a-e6ae-45f2-a8c9-d86bdfbe300f', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '4', 'Grade 4', 4),
           ('7cf58a70-7a97-43e0-a13a-2833b058f145', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '5', 'Grade 5', 5),
           ('53245fb7-391c-4ba3-98bb-511945143669', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '6', 'Grade 6', 6),
           ('25852824-1004-47ce-ac96-bbe7f457d968', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '7', 'Grade 7', 7),
           ('cbfd82e6-ca41-4648-9849-397b6e87eeb6', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '8', 'Grade 8', 8),
           ('5cc864d2-8257-4824-a12e-7801c0941557', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '9', 'Grade 9', 9),
           ('56f63ec4-29cc-4f2e-ae3d-26fba543e623', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'Q', 'Result Pending', 0),
           ('70267810-87ab-4f75-8e7c-ab6fe08febcd', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'U', 'Unclassified', 0),
           ('ebadc0c0-55c0-4a46-9291-67b7b2e12d7b', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'X', 'No Result Awarded', 0),
           ('22b94a10-fecf-4e41-b605-831d49bf091c', '11A469CD-FEC7-4A28-85FD-086F2B898300', '11', 'Grade 1-1', 1.5),
           ('1ded4312-e827-4497-8c65-406b88130832', '11A469CD-FEC7-4A28-85FD-086F2B898300', '21', 'Grade 2-1', 2.25),
           ('7cf41314-594e-4a03-a394-abc27d0aa9a1', '11A469CD-FEC7-4A28-85FD-086F2B898300', '22', 'Grade 2-2', 3),
           ('be1a42a6-8030-4ac7-8f01-e3338891e707', '11A469CD-FEC7-4A28-85FD-086F2B898300', '32', 'Grade 3-2', 3.75),
           ('e3b55029-29eb-4f3b-89c2-1ead63c81bf8', '11A469CD-FEC7-4A28-85FD-086F2B898300', '33', 'Grade 3-3', 4.5),
           ('0484b95e-62d4-49ef-87be-04ed5c4de7d4', '11A469CD-FEC7-4A28-85FD-086F2B898300', '43', 'Grade 4-3', 5.25),
           ('7a596c7e-66b6-4773-ad4d-d82526a6775e', '11A469CD-FEC7-4A28-85FD-086F2B898300', '44', 'Grade 4-4', 6),
           ('5f7aa21c-e5f0-49c7-873d-1122c59d6168', '11A469CD-FEC7-4A28-85FD-086F2B898300', '54', 'Grade 5-4', 6.75),
           ('4e05d016-8033-4813-bd20-bed23f0b3484', '11A469CD-FEC7-4A28-85FD-086F2B898300', '55', 'Grade 5-5', 7.5),
           ('a50c751d-0fa4-4710-bba7-ad5c1616da5c', '11A469CD-FEC7-4A28-85FD-086F2B898300', '65', 'Grade 6-5', 8.25),
           ('69a70582-c680-491e-82eb-3869f54dee9e', '11A469CD-FEC7-4A28-85FD-086F2B898300', '66', 'Grade 6-6', 9),
           ('e832e914-9509-4973-8880-4dcc681c39c3', '11A469CD-FEC7-4A28-85FD-086F2B898300', '76', 'Grade 7-6', 9.75),
           ('78db692d-d5e8-4313-88e3-a671a0cef4e6', '11A469CD-FEC7-4A28-85FD-086F2B898300', '77', 'Grade 7-7', 10.5),
           ('ceb18800-1ab5-4d3f-a2e2-151133aad677', '11A469CD-FEC7-4A28-85FD-086F2B898300', '87', 'Grade 8-7', 11.25),
           ('a6b3886b-9cd2-48d8-b2af-d9c151b109b5', '11A469CD-FEC7-4A28-85FD-086F2B898300', '88', 'Grade 8-8', 12),
           ('b931fd38-0d56-41cf-8549-13a8c6cb5dcb', '11A469CD-FEC7-4A28-85FD-086F2B898300', '98', 'Grade 9-8', 12.75),
           ('1ab4f836-be22-4e09-b81e-f39dd27f4504', '11A469CD-FEC7-4A28-85FD-086F2B898300', '99', 'Grade 9-9', 13.5),
           ('df430062-5ad5-48ff-b595-b8890c4bcc66', '11A469CD-FEC7-4A28-85FD-086F2B898300', 'Q', 'Result Pending', 0),
           ('d0120e02-ecd5-4b48-b062-ad2bf506d27e', '11A469CD-FEC7-4A28-85FD-086F2B898300', 'U', 'Unclassified', 0),
           ('8a793c4e-79cd-4f78-9821-de11d375103f', '11A469CD-FEC7-4A28-85FD-086F2B898300', 'X', 'No Result Awarded', 0),
           ('fe5e9cca-94b5-4b95-b38a-e6f874dc49aa', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'D', 'Distinction', 4),
           ('ddbfbc55-26c8-4258-b81b-cfb8643d51d6', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'M', 'Merit', 3),
           ('426f24bb-cc06-4006-8b70-e9fe91658f1c', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'NC', 'Not classified', 1),
           ('cbf3315e-14c6-4d58-bacc-e5ff1f51e8dd', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'P', 'Pass', 2),
           ('400169af-8855-434d-b87f-3ca97b8dd87a', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'Q', 'Result Pending', 0),
           ('190498bb-cdf1-4bc0-9385-3bea5f6c4f16', '11A469CD-FEC7-4A28-85FD-086F2B898301', 'X', 'No Result Awarded', 0),
           ('a0e53b53-6244-4e67-8f25-9ec470cd1a98', '11A469CD-FEC7-4A28-85FD-086F2B898302', 'NC', 'Not classified', 0),
           ('14150f40-1ae1-4080-bf5a-6067f73f6c8b', '11A469CD-FEC7-4A28-85FD-086F2B898302', 'P', 'Pass', 1),
           ('3683a4b5-b6a4-4204-bfa9-a4d9bf4b03d3', '11A469CD-FEC7-4A28-85FD-086F2B898302', 'Q', 'Result Pending', 0),
           ('d78935a9-0860-4df7-a2db-4afa3c5bbb61', '11A469CD-FEC7-4A28-85FD-086F2B898302', 'X', 'No Result Awarded', 0),
           ('574cc963-97d3-4967-9821-3f3569f3b29b', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'A', 'Grade A', 8),
           ('28effb0a-8dae-4fc6-b862-4aaf6d08078c', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'A*', 'Grade A*', 9),
           ('951498fc-72ff-4354-b1a7-9ea372c143fd', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'B', 'Grade B', 7),
           ('58ef486a-da50-41ed-b3bc-e462cf468984', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'C', 'Grade C', 5),
           ('5cd71f73-41ee-4705-8059-867a97a56d96', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'C*', 'Grade C*', 6),
           ('cdde45d4-4b2f-401f-a821-3e5fe65d3e8c', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'D', 'Grade D', 4),
           ('cf0b6693-b425-455b-a143-ef8af7a6619c', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'E', 'Grade E', 3),
           ('b7da3e2f-bcaf-440a-905f-7824a5d4d154', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'F', 'Grade F', 2),
           ('626d139d-ef43-41c3-9685-7146f1a86357', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'G', 'Grade G', 1),
           ('a4ae1595-9558-4e95-97d1-24a1d56064f6', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'Q', 'Result Pending', 0),
           ('16c94958-77df-4ce7-bc15-73ffbbb76445', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'U', 'Unclassified', 0),
           ('8025e152-abc1-4635-bc01-7072ee5adf26', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'X', 'No Result Awarded', 0),
           ('15b8597f-9ed8-4a3a-9d0f-7e7ff728f3a3', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'a', 'Grade a', 8),
           ('ce9ff93c-0ae4-47d9-ad26-ef631020b429', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'a*', 'Grade a*', 9),
           ('85881378-5f05-440c-a9a4-ec05cade9f6d', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'b', 'Grade b', 7),
           ('37db4e6b-ac24-47b5-b0e0-a72ce7c5bb26', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'c', 'Grade c', 5),
           ('22e80ac5-bdc0-49d1-a5f7-69dc514e2c4e', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'c*', 'Grade c*', 6),
           ('7bbb5d1a-7d83-47de-b01a-946aa2c3b831', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'd', 'Grade d', 4),
           ('61c54fa4-2fad-4d39-972b-48183425f276', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'e', 'Grade e', 3),
           ('9fd1bf0a-36b5-4e6c-a230-b4e8de977ce9', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'f', 'Grade f', 2),
           ('9f5ecedf-aded-46be-a898-ffb0ce56ff73', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'g', 'Grade g', 1),
           ('ad74e30f-1e77-4fc0-a9a4-c5fc7675a538', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'q', 'Result Pending', 0),
           ('faa658cd-a9a1-4ce1-9ae0-9d27cc750f7a', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'u', 'Unclassified', 0),
           ('1d5773a4-478c-42bd-8d1e-f22c99f27c14', '11A469CD-FEC7-4A28-85FD-086F2B898304', 'x', 'No Result Awarded', 0),
           ('b0dd5a7b-757b-45f2-bc21-4d6990a2706c', '11A469CD-FEC7-4A28-85FD-086F2B898305', '10', 'Pass 10', 18),
           ('f2e2682c-67cc-415e-ad21-efac819cf18e', '11A469CD-FEC7-4A28-85FD-086F2B898305', '15', 'Pass 15', 17),
           ('8868be95-483d-4f5f-a755-34f065e355d4', '11A469CD-FEC7-4A28-85FD-086F2B898305', '20', 'Pass 20', 16),
           ('5293b142-6f1d-4c82-ac18-a1c9e929d6d6', '11A469CD-FEC7-4A28-85FD-086F2B898305', '25', 'Pass 25', 15),
           ('925eeac7-e0c8-4b5c-8f42-c4f0b037c683', '11A469CD-FEC7-4A28-85FD-086F2B898305', '30', 'Pass 30', 14),
           ('33432d4c-c8b7-48ee-8e99-b3fbaacb53c7', '11A469CD-FEC7-4A28-85FD-086F2B898305', '35', 'Pass 35', 13),
           ('f3600688-bc76-44dc-a987-69cb45909c6d', '11A469CD-FEC7-4A28-85FD-086F2B898305', '40', 'Pass 40', 12),
           ('0cf0ef2a-3d53-4ce1-8274-9a1fff9277cb', '11A469CD-FEC7-4A28-85FD-086F2B898305', '45', 'Pass 45', 11),
           ('bcca28a0-f4b1-413e-b42a-ce01863eed4f', '11A469CD-FEC7-4A28-85FD-086F2B898305', '5', 'Pass 05', 19),
           ('7b3ffb4b-391a-4769-897b-6e064f525b18', '11A469CD-FEC7-4A28-85FD-086F2B898305', '50', 'Pass 50', 10),
           ('c00fa825-94cc-46e5-b832-20179f5c6713', '11A469CD-FEC7-4A28-85FD-086F2B898305', '55', 'Pass 55', 9),
           ('fdb13e93-f828-4deb-81f0-015230785270', '11A469CD-FEC7-4A28-85FD-086F2B898305', '60', 'Pass 60', 8),
           ('8501ebe7-8862-43a5-ac1c-dc3c358af690', '11A469CD-FEC7-4A28-85FD-086F2B898305', '65', 'Pass 65', 7),
           ('1dc6409e-35e8-4877-89dd-baad38c9345c', '11A469CD-FEC7-4A28-85FD-086F2B898305', '70', 'Pass 70', 6),
           ('d95b39f2-ec95-4700-8e25-88509fffcc7e', '11A469CD-FEC7-4A28-85FD-086F2B898305', '75', 'Pass 75', 5),
           ('ea6dc21a-b9cc-43f5-afaa-af2679b94a08', '11A469CD-FEC7-4A28-85FD-086F2B898305', '80', 'Pass 80', 4),
           ('272a2347-9cef-4514-b8c0-574bb5b9b386', '11A469CD-FEC7-4A28-85FD-086F2B898305', '85', 'Pass 85', 3),
           ('9cb846c2-6faa-43a8-adb3-b13bfd65bac7', '11A469CD-FEC7-4A28-85FD-086F2B898305', '90', 'Pass 90', 2),
           ('e404021f-a811-454b-8beb-af76937c3071', '11A469CD-FEC7-4A28-85FD-086F2B898305', '95', 'Pass 95', 1),
           ('eccc6645-057b-4595-a59b-02cd9f97999a', '11A469CD-FEC7-4A28-85FD-086F2B898305', 'Q', 'Result Pending', 0),
           ('adcf8fca-cbd2-4880-b938-0302ade5a144', '11A469CD-FEC7-4A28-85FD-086F2B898305', 'U', 'Unclassified', 0),
           ('1339f883-b6b1-46b1-ae69-8375f9fa9449', '11A469CD-FEC7-4A28-85FD-086F2B898305', 'X', 'No Result Awarded', 0),
           ('381f7b75-3516-4ca8-b74b-ceece97db240', '11A469CD-FEC7-4A28-85FD-086F2B898306', 'M1', 'Merit L1', 2),
           ('56a93fbd-a7c0-42a3-9257-8bf4a048d608', '11A469CD-FEC7-4A28-85FD-086F2B898306', 'P1', 'Pass L1', 1),
           ('33b00e12-3952-4e22-8f3c-a6c550e80f28', '11A469CD-FEC7-4A28-85FD-086F2B898306', 'Q', 'Result Pending', 0),
           ('53b8907c-7b6e-41f9-a726-84f293feb762', '11A469CD-FEC7-4A28-85FD-086F2B898306', 'U', 'Unclassified', 0),
           ('ea43f627-f65f-4d2d-ba51-6258fd10ad5d', '11A469CD-FEC7-4A28-85FD-086F2B898306', 'X', 'No Result Awarded', 0),
           ('862aee51-ac29-4853-bb49-d4d26e5d0919', '11A469CD-FEC7-4A28-85FD-086F2B898307', '3', 'Grade 3', 1),
           ('bcb6d43c-a757-4090-a584-3415949902d0', '11A469CD-FEC7-4A28-85FD-086F2B898307', '4', 'Grade 4', 2),
           ('316b9fb1-9f35-48bc-b052-ad5230bf4748', '11A469CD-FEC7-4A28-85FD-086F2B898307', '5', 'Grade 5', 3),
           ('f26538a1-f4d2-460f-b59b-5dfaa66ba167', '11A469CD-FEC7-4A28-85FD-086F2B898307', '6', 'Grade 6', 4),
           ('eb58d9c0-8d41-49fb-8fdd-0d1987b2b2f2', '11A469CD-FEC7-4A28-85FD-086F2B898307', '7', 'Grade 7', 5),
           ('a259ab05-7ba2-4a05-a425-ee39f2d90197', '11A469CD-FEC7-4A28-85FD-086F2B898307', '8', 'Grade 8', 6),
           ('a9d74654-aaff-4c8a-a01a-ffec991dcc6e', '11A469CD-FEC7-4A28-85FD-086F2B898307', '9', 'Grade 9', 7),
           ('ff567828-90a3-482d-b595-817dc8757075', '11A469CD-FEC7-4A28-85FD-086F2B898307', 'Q', 'Result Pending', 0),
           ('99f7a6cc-2d60-4a64-bdda-69cde122fbb6', '11A469CD-FEC7-4A28-85FD-086F2B898307', 'U', 'Unclassified', 0),
           ('014d5f07-c8fd-48f4-b313-551b960fb8da', '11A469CD-FEC7-4A28-85FD-086F2B898307', 'X', 'No Result Awarded', 0),
           ('e332dbae-b31a-458c-9d00-00a01d6a9387', '11A469CD-FEC7-4A28-85FD-086F2B898308', '1', 'Grade 1', 1),
           ('edc52a24-ceba-4ec5-9612-86cf4be69eb9', '11A469CD-FEC7-4A28-85FD-086F2B898308', '2', 'Grade 2', 2),
           ('8544e0b8-f78a-4068-9fe2-b06fd43fd044', '11A469CD-FEC7-4A28-85FD-086F2B898308', '3', 'Grade 3', 3),
           ('e927e34d-63fa-4148-aaaa-6fe8f4466a50', '11A469CD-FEC7-4A28-85FD-086F2B898308', '4', 'Grade 4', 4),
           ('d3a61ee9-46da-4abb-a39e-92ffacff52d5', '11A469CD-FEC7-4A28-85FD-086F2B898308', '5', 'Grade 5', 5),
           ('466ae73c-c748-4b5a-b8d0-9afa7b94e024', '11A469CD-FEC7-4A28-85FD-086F2B898308', 'Q', 'Result Pending', 0),
           ('0200fc6b-29af-461e-a175-b9e94acc3979', '11A469CD-FEC7-4A28-85FD-086F2B898308', 'U', 'Unclassified', 0),
           ('c492b04e-d1c5-4c2c-878f-ded90dd18e54', '11A469CD-FEC7-4A28-85FD-086F2B898308', 'X', 'No Result Awarded', 0),
           ('8d3ab5c7-c017-45bf-8ae3-8f45af434bbb', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'a', 'Grade a', 270),
           ('b93b057a-821a-49c7-8e60-978e47c382e8', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'a*', 'Grade a*', 300),
           ('ae4b59bc-fe01-41e2-a08c-733d1306804a', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'b', 'Grade b', 240),
           ('fe34ca03-6731-4a39-a0e6-f41062b3fd8c', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'c', 'Grade c', 210),
           ('4972a7a5-64c9-42f2-9e83-3a9ddeac90ca', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'd', 'Grade d', 180),
           ('dd43e170-8efe-43f4-beed-5693d2066fac', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'e', 'Grade e', 150),
           ('a0019320-7a38-44e1-beb8-8b7e10ddb0f2', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'Q', 'Pending', 0),
           ('fcf3a93b-dd58-4102-a2e6-290ebd031805', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'u', 'Unclassified', 0),
           ('039dbefa-d68c-4001-abea-c028adab2fa0', '11A469CD-FEC7-4A28-85FD-086F2B8982DC', 'X', 'No Result', 0),
           ('a817dd62-f83e-4e44-a7a9-694de132a716', '11A469CD-FEC7-4A28-85FD-086F2B898309', '43', 'Grade 4-3', 1),
           ('3f28bace-7187-4fbd-b593-8de4f6c9cd69', '11A469CD-FEC7-4A28-85FD-086F2B898309', '44', 'Grade 4-4', 2),
           ('25b7a813-71ca-4de7-81ed-f85df7aaa483', '11A469CD-FEC7-4A28-85FD-086F2B898309', '54', 'Grade 5-4', 3),
           ('e685906e-e29e-4f5c-9534-28b006353f34', '11A469CD-FEC7-4A28-85FD-086F2B898309', '55', 'Grade 5-5', 4),
           ('367ab989-55ff-4a67-9453-48a43cf5402a', '11A469CD-FEC7-4A28-85FD-086F2B898309', '65', 'Grade 6-5', 5),
           ('b3ca9096-5f52-460e-b284-c2d53ea9d645', '11A469CD-FEC7-4A28-85FD-086F2B898309', '66', 'Grade 6-6', 6),
           ('4c1f74cb-7aa1-42e9-9b7f-2f9a01ad9070', '11A469CD-FEC7-4A28-85FD-086F2B898309', '76', 'Grade 7-6', 7),
           ('4d543ab5-c3b6-402d-8056-be21d7caace9', '11A469CD-FEC7-4A28-85FD-086F2B898309', '77', 'Grade 7-7', 8),
           ('9a1b4c05-15b9-4d0d-8aa1-38991e719ec8', '11A469CD-FEC7-4A28-85FD-086F2B898309', '87', 'Grade 8-7', 9),
           ('bd90f59c-9112-42c5-a95b-8ea7215c3e64', '11A469CD-FEC7-4A28-85FD-086F2B898309', '88', 'Grade 8-8', 10),
           ('3f44a4d5-c80e-4633-b2a5-ce11f344889e', '11A469CD-FEC7-4A28-85FD-086F2B898309', '98', 'Grade 9-8', 11),
           ('5add07ac-71c6-418c-9763-c55bd7ef4a07', '11A469CD-FEC7-4A28-85FD-086F2B898309', '99', 'Grade 9-9', 12),
           ('9ccc4ed4-b10c-4f48-a6e6-ca63887365e8', '11A469CD-FEC7-4A28-85FD-086F2B898309', 'Q', 'Result Pending', 0),
           ('d9843a55-6a3e-4c91-bf0e-ff982ba5da17', '11A469CD-FEC7-4A28-85FD-086F2B898309', 'U', 'Unclassified', 0),
           ('93ba57e9-e333-4669-b2ed-4baf22b10bba', '11A469CD-FEC7-4A28-85FD-086F2B898309', 'X', 'No Result Awarded', 0),
           ('fbddced0-56d5-4865-97ec-3efc236c8e1d', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '11', 'Grade 1-1', 1),
           ('a14248cb-becc-4b82-9d26-266576bce60f', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '21', 'Grade 2-1', 2),
           ('167a8359-c69d-4530-a898-38b497cda5d2', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '22', 'Grade 2-2', 3),
           ('2e748571-55a0-4b46-9228-75017d90d897', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '32', 'Grade 3-2', 4),
           ('becb6784-0096-4a80-9de4-543a29d2d2de', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '33', 'Grade 3-3', 5),
           ('6eb8f8c1-e745-49b8-8666-96ee4a028a03', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '43', 'Grade 4-3', 6),
           ('62669c20-9fce-4026-a0ef-87d91678a0ab', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '44', 'Grade 4-4', 7),
           ('59c3cfdd-bd75-4c40-b3ad-acfb4dc04fd7', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '54', 'Grade 5-4', 8),
           ('85fdcb0f-d7c7-4664-9d66-d630abd80a7c', '11A469CD-FEC7-4A28-85FD-086F2B89830A', '55', 'Grade 5-5', 9),
           ('901ee784-ba82-4686-8ed2-241a8f53a88a', '11A469CD-FEC7-4A28-85FD-086F2B89830A', 'Q', 'Result Pending', 0),
           ('9e6435a3-ec61-4ba9-aa89-3f2d6c30f499', '11A469CD-FEC7-4A28-85FD-086F2B89830A', 'U', 'Unclassified', 0),
           ('287d2a2c-c9c5-4491-8fbe-2f6cf8156ef3', '11A469CD-FEC7-4A28-85FD-086F2B89830A', 'X', 'No Result Awarded', 0),
           ('83b2325e-f97a-414d-b659-a950f0e6cecb', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'A', 'Grade A', 4),
           ('6546b60a-0aaa-4be6-ba85-9ac12badf36e', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'A*', 'Grade A*', 5),
           ('1ebb83d2-866b-43a2-b966-010d2072d083', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'B', 'Grade B', 3),
           ('00c1209a-f7a8-458c-a1ff-7072f16044a9', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'C', 'Grade C', 2),
           ('6b1ace47-dfc0-4a70-9022-0bdb5dd18481', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'D', 'Grade D', 1),
           ('20dd6768-e48d-4caf-b5c5-1d1b5461c2ee', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'Q', 'Result Pending', 0),
           ('37b52f87-eefc-46a0-97a3-19a6153ab9b6', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'U', 'Unclassified', 0),
           ('fd1d7744-e049-4d7d-abfa-65494ccf12d4', '11A469CD-FEC7-4A28-85FD-086F2B89830B', 'X', 'No Result Awarded', 0),
           ('05476b44-74a0-4be5-a34a-97702b33a1a6', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'C', 'Grade C', 5),
           ('1e1ca8fc-b3e5-4fd8-bc03-bc8688cc939f', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'D', 'Grade D', 4),
           ('1dfe0109-8a96-4d5d-babb-b8948351a15e', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'E', 'Grade E', 3),
           ('820ad1bc-249f-4cb2-b42d-59f4b8aaa3b3', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'F', 'Grade F', 2),
           ('407a916b-83af-4d7d-9792-093f0ca00b49', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'G', 'Grade G', 1),
           ('e6c2d5f4-026b-4e8c-9326-677a80d79100', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'Q', 'Result Pending', 0),
           ('7c46bb15-9146-46aa-a53a-a5e22d1cece8', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'U', 'Unclassified', 0),
           ('b9fea7b2-9ab5-4077-8eaa-243e1d5122e4', '11A469CD-FEC7-4A28-85FD-086F2B89830C', 'X', 'No Result Awarded', 0),
           ('b6bb195f-0a91-4b59-ae44-bc0ec46ea611', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'a', 'Grade a', 4),
           ('f5b6d6e7-fd40-4944-800a-f8f6d6a208df', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'a*', 'Grade a*', 5),
           ('f4d04200-0ca4-44cd-8f5b-97eb0e906ae2', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'b', 'Grade b', 3),
           ('9123cab5-1780-40ef-96b9-368ad5f36f4e', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'c', 'Grade c', 2),
           ('fbcb8edb-dc97-407b-a794-4bd7c73000ff', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'd', 'Grade d', 1),
           ('51b47baa-7d25-4437-865c-b413474cc752', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'q', 'Result Pending', 0),
           ('44a0e37f-d3ad-493a-b2ef-d97f8dcce1b2', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'u', 'Unclassified', 0),
           ('f821efdd-b4a8-4d52-b6e7-8ad0f5af60e8', '11A469CD-FEC7-4A28-85FD-086F2B89830D', 'x', 'No Result Awarded', 0),
           ('384b0760-bb8d-4e7f-8760-164bd04b4633', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'd', 'Grade d', 4),
           ('a6590a6a-c97a-424d-bd14-f1be1fb9e21e', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'e', 'Grade e', 3),
           ('fb6667e6-4799-4fd9-9ab8-5709c43096b9', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'f', 'Grade f', 2),
           ('afa69ae2-7425-4b34-9859-5edfb9f26595', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'g', 'Grade g', 1),
           ('d6f2402f-ad87-4bab-857d-df91e07def60', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'q', 'Result Pending', 0),
           ('428453f7-8635-41de-ba40-a12debdec386', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'u', 'Unclassified', 0),
           ('c85c0cf5-000c-4d85-8b91-b7e4c48ba50e', '11A469CD-FEC7-4A28-85FD-086F2B89830E', 'x', 'No Result Awarded', 0),
           ('d072e6c5-d2ec-4302-a566-2673fed49167', '11A469CD-FEC7-4A28-85FD-086F2B89830F', '**', 'Grade A*A*', 8),
           ('1e048cee-7446-4b2b-882d-ff52e5a1d4c3', '11A469CD-FEC7-4A28-85FD-086F2B89830F', '*A', 'Grade A*A', 7),
           ('feb10cd9-4a9f-427d-8a40-4eee0ac8520c', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'AA', 'Grade AA', 6),
           ('09d6c6dc-c740-4620-b131-2dfd18c70d37', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'AB', 'Grade AB', 5),
           ('8d83a8f4-eb09-4dac-a4dc-2b55fbe8b39a', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'BB', 'Grade BB', 4),
           ('0882ce65-7ead-4e3e-8c82-3e6663caf457', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'BC', 'Grade BC', 3),
           ('f5c1d177-9007-480e-b40e-b90baaee7b4d', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'CC', 'Grade CC', 2),
           ('fa8400ba-4bc2-48ff-919b-8feca9a41b26', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'CD', 'Grade CD', 1),
           ('f75b1640-b75f-4232-a021-a5a0de448e22', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'Q', 'Result Pending', 0),
           ('bdb59233-78c7-4d31-be0a-85db61403a9a', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'U', 'Unclassified', 0),
           ('c36b1a5a-8311-4f16-9fc6-b8a9be92c97a', '11A469CD-FEC7-4A28-85FD-086F2B89830F', 'X', 'No Result Awarded', 0),
           ('a0fe3110-44c6-44e8-9c25-3d0e58c83586', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'CC', 'Grade CC', 9),
           ('ab7df571-dd68-47bc-9bc1-b95cb3782754', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'CD', 'Grade CD', 8),
           ('0c095a62-703a-4cdc-ae6e-338bcfcffbab', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'DD', 'Grade DD', 7),
           ('c4b5bc56-08d3-4c91-9690-690c48bf4aae', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'DE', 'Grade DE', 6),
           ('658936da-7c41-436d-99f2-55424aeb8b94', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'EE', 'Grade EE', 5),
           ('b58f39c9-c387-459a-8eb2-1b44a76b67c0', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'EF', 'Grade EF', 4),
           ('b991b1d3-7ad3-4ee9-b853-698334cef1a7', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'FF', 'Grade FF', 3),
           ('9766fde3-77fe-42a4-99a6-006b9061309e', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'FG', 'Grade FG', 2),
           ('8df7fc12-be40-48a6-8198-3bd657f89d5e', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'GG', 'Grade GG', 1),
           ('2041c225-bd7f-4860-a654-6c438455237c', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'Q', 'Result Pending', 0),
           ('6060122b-c512-491b-bc61-0e64dd8683e2', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'U', 'Unclassified', 0),
           ('84802415-4861-414d-a943-0a518004c12f', '11A469CD-FEC7-4A28-85FD-086F2B898310', 'X', 'No Result Awarded', 0),
           ('55993cf0-1cc8-44ee-b6fd-ec365ceb6e39', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'D', 'Distinction', 3),
           ('d268b9cf-ae7f-4e9b-853c-2a9dc915ab70', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'M', 'Merit', 2),
           ('42a24afe-2fda-4ef4-a593-d8f951edf734', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'P', 'Pass', 1),
           ('8a1e937b-b67c-482e-a7e8-9ae7b0ad394f', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'Q', 'Result Pending', 0),
           ('ae632f3a-4eb0-4d0d-93d7-423ff5d73246', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'U', 'Unclassified', 0),
           ('188d6524-f59e-427b-83c8-360871d22ef4', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'X', 'No Result Awarded', 0),
           ('6f05c55d-289f-40fb-95da-7337e46463ee', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'D', 'Distinction', 3),
           ('f8f50b04-bdc4-43bf-ad25-49969ec72d4a', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'D*', 'Distinction*', 4),
           ('a2be26f6-5649-44e4-aabd-c715cdb31a84', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'M', 'Merit', 2),
           ('229483e7-5e00-4b27-a08f-df430fa00198', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'P', 'Pass', 1),
           ('2c99c83f-4316-4162-898b-d71bfd7de0f7', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'Q', 'Result Pending', 0),
           ('67ee7d45-c829-4c51-b55a-a9933e3a9705', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'U', 'Unclassified', 0),
           ('0d0bd019-6f9e-4f92-b2de-58c6b570e7d5', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'X', 'No Result Awarded', 0),
           ('b6c35d41-ad53-404c-89de-b9c58fe33d83', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'A', 'Grade A', 84),
           ('bf3769e1-5937-4d6c-afa4-43bdf08d8395', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'A*', 'Grade A*', 102),
           ('556ef560-dde0-4683-9480-6be21456e2d1', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'B', 'Grade B', 57),
           ('69123205-bbe2-4152-bf9b-9a5d414b10bc', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'Q', 'Pending', 0),
           ('6c1824d8-0d13-459d-95e2-ba248778a984', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'U', 'Unclassified', 0),
           ('2c3a9572-65da-485f-8b09-f5a7e7ef2072', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'X', 'No Result', 0),
           ('45fe61f6-eaad-41e6-b4f7-df012e1f65fa', '11A469CD-FEC7-4A28-85FD-086F2B898313', 'D', 'Distinction', 2),
           ('1e464bc9-63a1-4259-9c47-6a1cda2c4381', '11A469CD-FEC7-4A28-85FD-086F2B898313', 'P', 'Pass', 1),
           ('a6b77b4c-d306-404f-9c66-da44fe70cfb8', '11A469CD-FEC7-4A28-85FD-086F2B898313', 'Q', 'Result Pending', 0),
           ('db26b9d8-247d-4e87-9bd8-ddf5205d840c', '11A469CD-FEC7-4A28-85FD-086F2B898313', 'U', 'Unclassified', 0),
           ('3a4fa6b0-55bf-44ad-9cc5-0dd0c250cde6', '11A469CD-FEC7-4A28-85FD-086F2B898313', 'X', 'No Result Awarded', 0),
           ('cd282f0d-3cef-4d09-8ae2-76c04a22682e', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'a', 'Grade a', 8),
           ('8d25dcab-af6f-4eb7-9537-ac6a16246fe1', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'b', 'Grade b', 7),
           ('3fa57393-aad9-43fd-9382-bd0dd7948685', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'c', 'Grade c', 5),
           ('facfad13-bc8f-44ed-bf59-7d72910950a4', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'c*', 'Grade c*', 6),
           ('21aacbcc-15a2-4929-bb55-cd6ebf75a0f3', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'd', 'Grade d', 4),
           ('3830f5c7-608c-4161-9508-1ffa1e604f25', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'e', 'Grade e', 3),
           ('4f633ae6-3728-48c2-9f8d-44cf517dc92a', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'f', 'Grade f', 2),
           ('21ab89f7-8cb5-4d3c-8582-d9c493a0c658', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'g', 'Grade g', 1),
           ('9eccf2b5-328e-4646-8c29-9a14286b6d75', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'q', 'Result Pending', 0),
           ('97c251a5-b1a4-40dc-a504-a441bb22822c', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'u', 'Unclassified', 0),
           ('e7906c88-233d-4cc8-8c30-6a5b91756c45', '11A469CD-FEC7-4A28-85FD-086F2B898314', 'x', 'No Result Awarded', 0),
           ('3829941f-f306-4572-bc32-4604acef650f', '11A469CD-FEC7-4A28-85FD-086F2B898315', '**', 'Grade A*A*', 17),
           ('4f10c955-e3f0-474a-81a5-ca29dc76b8b2', '11A469CD-FEC7-4A28-85FD-086F2B898315', '*A', 'Grade A*A', 16),
           ('780b3f62-7cfe-4380-aea2-9866288a60bc', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'AA', 'Grade AA', 15),
           ('810c4d9b-1398-43e2-a9a3-702fca0b35eb', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'AB', 'Grade AB', 14),
           ('26d2cb42-6dd0-44a9-b258-708959d74d5c', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'B1', 'Grade BC*', 12),
           ('f7c797f9-a0d3-45b0-b994-9a10c596e449', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'BB', 'Grade BB', 13),
           ('45c77b7f-dfe8-4ce9-8eb7-e5947da8d9f3', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'C1', 'Grade C*C*', 11),
           ('64874103-a14f-4916-9651-c362f2b68b0a', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'C2', 'Grade C*C', 10),
           ('1fefbf51-d4d9-4088-83cf-7a9af054f3f3', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'CC', 'Grade CC', 9),
           ('c13be61a-8769-4401-a2e3-5b1112af6745', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'CD', 'Grade CD', 8),
           ('b9e96796-bdc8-4a36-bec3-03aaf6c312d9', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'DD', 'Grade DD', 7),
           ('4491d415-e8be-4696-baf4-bd74085d8d0a', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'DE', 'Grade DE', 6),
           ('70ad983b-67a6-4893-a531-e1633b26a8e7', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'EE', 'Grade EE', 5),
           ('7886cc5e-8a43-4a0a-b8a5-355e336eac10', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'EF', 'Grade EF', 4),
           ('d65c5889-b311-4ff6-bf2e-f5645cb8ea30', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'FF', 'Grade FF', 3),
           ('1a49a382-d314-4b42-8c91-5cfa36640c6e', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'FG', 'Grade FG', 2),
           ('d06667d6-1041-4f54-aaf3-e754504f05d8', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'GG', 'Grade GG', 1),
           ('89b31e87-3010-4a55-9fba-680f337b5003', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'Q', 'Result Pending', 0),
           ('dcb4fbef-030c-4139-aad8-79cc0095e261', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'U', 'Unclassified', 0),
           ('7025ccc5-d15c-41aa-a1b4-0fa6f179bb06', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'X', 'No Result Awarded', 0),
           ('081d6306-949a-4a5d-94f9-f032846e15d3', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'a', 'Grade a', 6),
           ('81ed6765-8f11-4676-9ccd-80fb79c945b1', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'b', 'Grade b', 5),
           ('65b09f98-4fa1-49f6-b87c-cae2925c8d04', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'c', 'Grade c', 3),
           ('facece78-c41f-4ded-aa86-da0044fe490b', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'c*', 'Grade c*', 4),
           ('39d663b1-1f13-415c-a69b-fd608b9ea4d9', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'd', 'Grade d', 2),
           ('3fc83ce5-929d-4330-a848-48c569750e7d', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'e', 'Grade e', 1),
           ('85db7125-a6d6-4c8e-89f0-9d603f56e725', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'q', 'Result Pending', 0),
           ('db546eec-2612-469b-8270-65f1e8a7f6ff', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'u', 'Unclassified', 0),
           ('88beaa54-729d-48c4-bcd2-d79e01439921', '11A469CD-FEC7-4A28-85FD-086F2B898316', 'x', 'No Result Awarded', 0),
           ('3e4bb261-120d-4d06-8388-e373482727c0', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'c', 'Grade c', 5),
           ('43d88d69-9a90-42e9-998e-0c1638deb2be', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'c*', 'Grade c*', 6),
           ('b4184f7a-7dc9-478f-8455-53b45f078bb3', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'd', 'Grade d', 4),
           ('82c9d4ba-bc26-4b2c-bde0-7357642f5e72', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'e', 'Grade e', 3),
           ('64aaf9a1-778a-4809-95a3-b9d69af89e78', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'f', 'Grade f', 2),
           ('be1bcd90-c756-4b71-b34a-a3d0b62d8ad1', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'g', 'Grade g', 1),
           ('85838717-c5b3-49d2-82a9-128859204286', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'q', 'Result Pending', 0),
           ('e09b6058-4053-40a4-9605-f4e1bbd9279d', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'u', 'Unclassified', 0),
           ('ba4fef4f-c240-4af5-a92f-33f9a2c5debf', '11A469CD-FEC7-4A28-85FD-086F2B898317', 'x', 'No Result Awarded', 0),
           ('1dd20295-9a0b-404c-8e19-21fa078a69a4', '11A469CD-FEC7-4A28-85FD-086F2B898318', '11', 'Grade 1-1', 1),
           ('c4ea6c15-da98-461b-b891-0c9a808eae70', '11A469CD-FEC7-4A28-85FD-086F2B898318', '22', 'Grade 2-2', 2),
           ('b11226f1-de60-4f72-b0b8-b74c15b6d31d', '11A469CD-FEC7-4A28-85FD-086F2B898318', '33', 'Grade 3-3', 3),
           ('f0ac6e70-3543-42df-8b89-50eeca97d683', '11A469CD-FEC7-4A28-85FD-086F2B898318', '44', 'Grade 4-4', 4),
           ('31e18a71-263e-497d-923a-047ebaebfec1', '11A469CD-FEC7-4A28-85FD-086F2B898318', '55', 'Grade 5-5', 5),
           ('2057606a-dfbf-4acd-ba83-aa081d3ebb1f', '11A469CD-FEC7-4A28-85FD-086F2B898318', '66', 'Grade 6-6', 6),
           ('b13db8c7-2c61-43d8-9c82-7b9883609fcc', '11A469CD-FEC7-4A28-85FD-086F2B898318', '77', 'Grade 7-7', 7),
           ('6199360f-1dba-4e38-b035-c2e1ff3a04c7', '11A469CD-FEC7-4A28-85FD-086F2B898318', '88', 'Grade 8-8', 8),
           ('1b616f8e-e8cc-42db-aa11-df783514e5ae', '11A469CD-FEC7-4A28-85FD-086F2B898318', '99', 'Grade 9-9', 9),
           ('250f79c4-acdd-40e1-82e2-0f38437f82c0', '11A469CD-FEC7-4A28-85FD-086F2B898318', 'Q', 'Result Pending', 0),
           ('e9cd1481-d4c8-467d-8185-e94784c338d2', '11A469CD-FEC7-4A28-85FD-086F2B898318', 'U', 'Unclassified', 0),
           ('65629fc6-b932-4a3f-9c5b-6511c2ba2f00', '11A469CD-FEC7-4A28-85FD-086F2B898318', 'X', 'No Result Awarded', 0),
           ('a73834a4-9894-4c4a-bd03-b0a7ce70fbad', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'D', 'Distinction', 3),
           ('9ede8f6b-6036-4a41-b368-db519a6a5001', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'M', 'Merit', 2),
           ('6d9172c5-347d-469c-8096-7f4a72bd773a', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'P', 'Pass', 1),
           ('b8c279da-659d-4563-9176-49f7ac17ce6a', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'Q', 'Result Pending', 0),
           ('398858d4-c03f-4a31-b972-f3ea2f70f7ff', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'U', 'Unclassified', 0),
           ('5ff95568-af91-416f-b4b1-73142dc22e7f', '11A469CD-FEC7-4A28-85FD-086F2B898319', 'X', 'No Result Awarded', 0),
           ('20176a1d-e7c2-4b9b-a362-c0b4d3cc9ad1', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'A', 'L1 Adv. Credit', 2),
           ('5c887750-772f-4362-aab3-b6c3c91f5ace', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'C', 'L1 Credit', 1),
           ('814cfec6-9495-4857-9626-76dd269c9a92', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'D', 'L2 Distinction', 5),
           ('3407bab7-f309-4313-ad4f-49f47cb24a2b', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'D*', 'L2 Distinction*', 6),
           ('3073a5ea-7cd0-4356-a528-85447c68f531', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'M', 'L2 Merit', 4),
           ('d9dbbe5d-d002-4956-8381-22ac3e7460d0', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'P', 'L2 Pass', 3),
           ('c27b6cca-3171-431f-91b6-9cf83cda1cac', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'Q', 'Result Pending', 0),
           ('7f080404-9794-4f7d-857c-26e056ca1efa', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'U', 'Unclassified', 0),
           ('5ab94ded-3b4e-4be1-b392-e55811f02dcb', '11A469CD-FEC7-4A28-85FD-086F2B89831A', 'X', 'No Result Awarded', 0),
           ('40eb2960-f065-46b1-ae18-d8c405df5042', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'A', 'L1 Adv. Credit', 2),
           ('a40b9f29-271f-447f-aadc-dc7fab0610aa', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'C', 'L1 Credit', 1),
           ('b756023c-492c-408a-81de-a0e93926b84e', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'D', 'L2 Distinction', 5),
           ('2e0ad781-c3f4-4188-a92c-8eb53331cf9a', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'D*', 'L2 Distinction*', 6),
           ('88b06be8-2b98-4033-a09b-bd8615816a45', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'M', 'L2 Merit', 4),
           ('b93853d2-03b4-4fe1-9637-daf8167f4b44', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'P', 'L2 Pass', 3),
           ('72892bcb-439d-4897-9e33-728f724a7416', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'Q', 'Result Pending', 0),
           ('96a519d5-7053-4c32-8a7b-6614c5b79ad3', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'U', 'Unclassified', 0),
           ('d6db8db4-4022-491e-b9c8-3da9fcac5e54', '11A469CD-FEC7-4A28-85FD-086F2B89831B', 'X', 'No Result Awarded', 0),
           ('3a20358d-c9c2-4f3b-a4ba-d0ec93b66ca7', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'BT', 'Below Threshold', 0),
           ('03ccc8d0-3802-4d85-b6f0-463716bd11a8', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'D', 'Distinction', 3),
           ('b296c49b-ae5e-465b-b43f-aca9d2ec24b3', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'M', 'Merit', 2),
           ('48181eb4-95a6-4046-b9dd-312b35e05abd', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'P', 'Pass', 1),
           ('8321a101-47b5-4a71-af31-ffba5da0e4ce', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'Q', 'Result Pending', 0),
           ('deec9034-10c7-41a8-9dc4-70b1996ec960', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'U', 'Unclassified', 0),
           ('b0b2c5a8-0131-4443-8099-d0d30533b5bb', '11A469CD-FEC7-4A28-85FD-086F2B89831C', 'X', 'No Result Awarded', 0),
           ('26ca9740-ddb5-4717-85e1-99ab6b4d6bcd', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'a', 'Grade a', 84),
           ('1cdbe12f-0d4c-4860-b908-4334ddd1068d', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'a*', 'Grade a*', 102),
           ('2ce6b8a1-72ec-49de-b22a-a055be6ac787', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'b', 'Grade b', 57),
           ('c9a1b4ee-966d-4271-8768-3918acb43341', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'Q', 'Pending', 0),
           ('50d2f04c-d8d4-447b-8e59-6f6445a36c20', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'u', 'Unclassified', 0),
           ('65fdbf77-bb35-4943-b0b6-d967cf5f6757', '11A469CD-FEC7-4A28-85FD-086F2B8982DE', 'X', 'No Result', 0),
           ('719a60cf-1d2d-4423-85a1-200052aba68a', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'A', 'Achieved', 2),
           ('4905c22e-fe04-4763-a534-a4a17b54230d', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'D', 'Distinction', 5),
           ('9371df81-1b60-4a78-ad39-a99cdb3ac682', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'M', 'Merit', 4),
           ('f70d2112-e250-4ab9-85c6-10afaaed2e4a', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'N', 'Not Achieved', 1),
           ('38996018-7d90-4a65-be37-5cf22b1ede86', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'P', 'Pass', 3),
           ('7e92fca6-83aa-4acd-9b91-9999b43f7b4b', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'Q', 'Result Pending', 0),
           ('abf624d1-2157-4ba4-9635-63aa5243182c', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'U', 'Unclassified', 0),
           ('64b9a79b-bac1-4129-9ae2-e1c711f88779', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'X', 'No Result Awarded', 0),
           ('bb4fe7d2-fbfa-4a39-b87c-8f1e034c8223', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'D2', 'Distinction L2', 5),
           ('c2f474e5-6855-4c02-a816-85cd7262a844', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'M1', 'Merit L1', 2),
           ('d9663e04-5c10-453d-9670-5e69454530c4', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'M2', 'Merit L2', 4),
           ('598eebf1-faad-4d58-a653-511fd9c2cb00', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'P1', 'Pass L1', 1),
           ('d3bde58c-18cf-4892-8bc2-3d90353cbfa3', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'P2', 'Pass L2', 3),
           ('694142fd-396f-4ced-a663-ab5fe5aee9d7', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'Q', 'Result Pending', 0),
           ('9d7806a3-4b42-4754-9ce5-21763540c528', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'U', 'Unclassified', 0),
           ('8b819618-168d-43d0-aeff-af32220f992a', '11A469CD-FEC7-4A28-85FD-086F2B89831E', 'X', 'No Result Awarded', 0),
           ('7eb9ce21-3371-485a-a149-fe796865f8cc', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'D1', 'Distinction L1', 3),
           ('865c2a6b-3f71-4787-845f-2df8218bd208', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'D2', 'Distinction L2', 6),
           ('855a03ce-f3ac-419c-9d0e-d1ee18b8dfb2', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'M1', 'Merit L1', 2),
           ('44e5e541-888c-466b-8461-1da0068594ec', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'M2', 'Merit L2', 5),
           ('0f9dc677-f632-4595-b327-3b0d1347ebab', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'P1', 'Pass L1', 1),
           ('ee471353-3273-4b94-8bb2-43fa02098b92', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'P2', 'Pass L2', 4),
           ('b2911d19-ebcc-45b1-af45-e52ad37df06d', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'Q', 'Result Pending', 0),
           ('f0b2ab48-7cb2-48dd-aee6-26b0e42a4d13', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'U', 'Unclassified', 0),
           ('2cd3807f-0c52-4c83-bb34-319680ee2bdb', '11A469CD-FEC7-4A28-85FD-086F2B89831F', 'X', 'No Result Awarded', 0),
           ('4a13c36a-aea0-4f46-aad9-ca34b79bbad5', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'DD', 'DD', 5),
           ('0a48ad97-6095-4a64-9d96-9aecd22bbea8', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'DM', 'DM', 4),
           ('24f2e133-da57-4808-8f3a-c0242874f5b0', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'MM', 'MM', 3),
           ('ac891f72-9e6e-47d7-85c9-faf48af96437', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'MP', 'MP', 2),
           ('4615a420-5517-431c-98fa-c784c54b6562', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'PP', 'PP', 1),
           ('e616fbaa-8122-4aab-a953-e19f58ab5228', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'Q', 'Result Pending', 0),
           ('650d6929-e53e-4ee0-9725-e494001d8c2b', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'U', 'Unclassified', 0),
           ('0119e02d-c201-4fca-a47f-be4991bd4d65', '11A469CD-FEC7-4A28-85FD-086F2B898320', 'X', 'No Result Awarded', 0),
           ('7a1a8aa6-b4f3-4dcc-886c-afde5c29f46c', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'A', 'Grade A', 91.33),
           ('eecf9846-dde2-4280-a05d-4a9c8de5ca08', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'A*', 'Grade A*', 102.17),
           ('ff851d8a-54ca-4911-9e4f-a627b11346fe', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'B', 'Grade B', 80.5),
           ('5c2fcfd2-9c4b-4fcf-944f-1df0b736305f', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'C', 'Grade C', 69.67),
           ('7683fad4-a7a6-4673-91b0-1c973e581c75', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'Q', 'Pending', 0),
           ('6e694bd8-56bf-406a-9aa2-ebf81b0e1123', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'U', 'Unclassified', 0),
           ('90e6125e-fe65-43b1-a55b-e87c656015b3', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'X', 'No Result', 0),
           ('50db0859-a9d5-4e03-871a-c84ae94fefe7', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'a', 'Grade a', 260),
           ('5e0ffee9-ff0b-4745-8ba5-556f79ba9379', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'a*', 'Grade a*', 290),
           ('0d2fd0ed-d520-457e-bb68-5d4b4a9b315e', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'b', 'Grade b', 230),
           ('f47ac555-58ed-47d5-b98f-68c1997a8515', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'c', 'Grade c', 200),
           ('454407fd-ac7a-4812-867b-7ce12436af51', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'Q', 'Pending', 0),
           ('45f3617f-88d0-4ab5-8953-83d81113c71a', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'u', 'Unclassified', 0),
           ('83e52e7d-e6ef-497a-b617-58facb65b501', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'X', 'No Result', 0),
           ('5088a82a-beac-47d7-aa24-88a288a50212', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'A', 'Grade A', 106.67),
           ('c987fc7d-7a52-4422-bbfc-34e1890dfc1e', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'B', 'Grade B', 93.33),
           ('a08e744b-33a1-471b-be45-40ce4926f52c', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'C', 'Grade C', 80),
           ('984408d6-e73e-4119-896b-e606e37e7d2c', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'D', 'Grade D', 66.67),
           ('cced3347-1ae7-4a26-a6cf-9799da5de9df', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'E', 'Grade E', 53.33),
           ('df8854a8-99db-4869-bca9-8762e04a3ca5', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'Q', 'Pending', 0),
           ('279b708c-9e68-4db0-8373-99987c8384af', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'U', 'Unclassified', 0),
           ('b3a3b1ef-4611-4f16-aff1-b1284a25613e', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'X', 'No Result', 0),
           ('7c2cf6e9-0fd3-4320-b269-0f5dd94d4309', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'a', 'Grade a', 5),
           ('efc82f1c-f489-4916-a109-cf4888c939a5', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'b', 'Grade b', 4),
           ('72805a05-d161-4063-bf2d-67f60811e219', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'c', 'Grade c', 3),
           ('4e22f7f5-b9be-4151-9fa7-572d6580b0fb', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'd', 'Grade d', 2),
           ('4272a198-8cfc-42a5-8afa-f5000e742167', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'e', 'Grade e', 1),
           ('7c446d4d-d381-4dd0-b459-5a91c5a52dfa', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'Q', 'Pending', 0),
           ('f610086b-39dd-484e-8dc4-4a966c4d476e', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'u', 'Unclassified', 0),
           ('0d279e3a-3ec7-4670-9b93-d09fedc28862', '11A467CD-FEC7-4A28-85FD-086F2B8982BF', 'X', 'No Result', 0),
           ('3208a7ca-d730-4b01-be73-1299d3430067', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'AA', 'Grade AA', 213.33),
           ('6107be5c-c81f-48e4-b04d-651642002f28', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'AB', 'Grade AB', 200),
           ('194eee59-d397-418f-bf2c-be9c402fa0e8', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'BB', 'Grade BB', 186.67),
           ('f8e1dc38-865f-48b5-8eb4-155d015548eb', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'BC', 'Grade BC', 173.33),
           ('20cf0c71-2fb9-435e-8ef3-2190fcfa75c4', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'CC', 'Grade CC', 160),
           ('24eeecb6-06ac-4724-af46-3654d0202c09', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'CD', 'Grade CD', 146.67),
           ('142aa978-9f59-4df0-bcc5-01a1b043a89f', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'DD', 'Grade DD', 133.33),
           ('3f122dfa-2522-4617-8808-55fa1ef639ba', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'DE', 'Grade DE', 120),
           ('c6999c98-8090-437f-8553-9a3ccff71dde', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'EE', 'Grade EE', 106.67),
           ('ebc3838d-b5c1-4256-8163-ba39268a30d9', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'Q', 'Pending', 0),
           ('59723fe7-ea23-4561-8754-84a3a329ea35', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'U', 'Unclassified', 0),
           ('554fd55b-a656-43e0-9076-86766f41b1a6', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'X', 'No Result', 0),
           ('58933e72-f82f-4021-82fc-f8576da6e032', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'D', 'Distinction', 3),
           ('81874c77-76a5-46ee-895a-ac36e7af617f', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'M', 'Merit', 2),
           ('c3bb5ff4-ac24-4e1a-91a6-c895376f71be', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'P', 'Pass', 1),
           ('176303fd-95f8-40e0-ad93-461cc0e6ff89', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'Q', 'Pending', 0),
           ('62db9f98-5768-408e-902f-f2166967ba00', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'U', 'Unclassified', 0),
           ('484c34f7-2b82-46eb-95e5-d3074ae54b68', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'X', 'No Result', 0),
           ('9181d200-9b8c-4bfd-bbcc-0317bb4d6d0a', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'd', 'Distinction', 3),
           ('0b73ab52-395b-4241-b3b1-9bba6b03a264', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'm', 'Merit', 2),
           ('ae1f4d13-742f-42c9-9827-a73f611c2597', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'p', 'Pass', 1),
           ('b58ac99c-93e3-49a5-963a-bb5fc25a3d53', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'Q', 'Pending', 0),
           ('04d5c9ac-fd3f-40c5-9a41-453d8817b801', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'u', 'Unclassified', 0),
           ('f3ab6a00-b7c8-4c43-a21a-458d3c460fa4', '11A467CD-FEC7-4A28-85FD-086F2B8982C2', 'X', 'No Result', 0),
           ('4be3cf9c-34ed-4e51-b2de-9e4dbdaab8d5', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'A', 'Grade A', 22),
           ('131cc026-eebe-4ffb-8f77-ea34b711ff8d', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'A*', 'Grade A*', 24.83),
           ('52c11082-23bf-4586-8dc0-f31171d596c7', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'B', 'Grade B', 19.17),
           ('34df32de-bf15-4d8e-b040-f77ffbf035ff', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'C', 'Grade C', 16.33),
           ('659fc2ac-ad54-42de-a7c6-984ad227ba2e', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'D', 'Grade D', 13.67),
           ('be630cb6-2cd7-4dcf-8e74-511782a15266', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'E', 'Grade E', 11),
           ('dcf63b3d-abf9-449e-a290-c7acf421f76d', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'F', 'Grade F', 8.5),
           ('4d1d04e7-de68-4660-80ea-5af8122eed87', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'G', 'Grade G', 6),
           ('3604f08a-8baf-49d4-a3cd-03a3b449f0b5', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'Q', 'Pending', 0),
           ('1d795790-5266-419e-bb30-1503f6126239', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'U', 'Unclassified', 0),
           ('788e6aa1-86df-4dd1-9322-7ddc3e3c137a', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'X', 'No Result', 0),
           ('7bba3fc6-bfa1-4518-8ee5-2c283ccf506f', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', '1', 'Entry 1', 10),
           ('39ecf5b9-bd6c-4b3e-a6b4-944ca71a8eb1', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', '2', 'Entry 2', 12),
           ('2865cb64-b2f1-4214-ac5b-e7e1f3f76211', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', '3', 'Entry 3', 14),
           ('84766df7-9d4a-4ec9-9268-d9496f8693bb', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'Q', 'Pending', 0),
           ('c1735643-2e84-41f4-94b5-d47b60b0630f', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'U', 'Unclassified', 0),
           ('78b62c89-d1b6-46f9-bea0-f01b03086d72', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'X', 'No Result', 0),
           ('053c3339-ecc7-4d0e-a3a2-a6882984fc1e', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'D', 'Distinction', 6.67),
           ('e2114501-57a7-4e25-8c55-adf634c24a3a', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'M', 'Merit', 5.47),
           ('197e01f9-eaff-4c40-b6d4-f1c23dd2e4b0', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'Q', 'Pending', 0),
           ('ec359d93-c4bc-44a7-b128-05b067dcfddb', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'U', 'Unclassified', 0),
           ('a6e95304-8e4f-4dc6-b1f9-a129da2ffee8', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'X', 'No Result', 0),
           ('76a2baf1-fe0d-484e-be89-3adc305c1c6d', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'A', 'Grade A', 10),
           ('82132afe-06ce-45da-ac34-9a82fb21006a', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'B', 'Grade B', 8),
           ('56a3d1c5-7810-48e3-bb0f-f85daf7966c8', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'C', 'Grade C', 6),
           ('5642a6cd-cf96-40f0-8269-ecd02b9eec9c', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'D', 'Grade D', 4),
           ('75cd5a35-5d8f-4488-8162-cc42afa339f7', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'E', 'Grade E', 2),
           ('8e211011-fc90-4141-bf90-9699b03f9bc4', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'N', 'Grade N', 0),
           ('19eee5c8-9533-47d2-9f6d-9bf1fa0e5b34', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'Q', 'Pending', 0),
           ('4462988d-b483-460a-bbb7-5a05fda918ad', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'U', 'Unclassified', 0),
           ('72abe490-555a-41a4-8b46-d46940200fee', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'X', 'No Result', 0),
           ('d8a3eb6f-0c3d-4931-8112-fbf5853974c4', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'a', 'Grade a', 5),
           ('16d0cc7b-f766-41f9-8b76-6bf1be795155', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'b', 'Grade b', 4),
           ('36123e76-22f1-4bc8-8cb2-14e1d4d37a05', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'c', 'Grade c', 3),
           ('9cf9983e-0cc8-4803-8baf-8f8f5cee4fac', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'd', 'Grade d', 2),
           ('ee0d91d4-a35f-4773-a0ad-f14fb99748bc', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'e', 'Grade e', 1),
           ('ff3f8b0f-33a0-471b-b8f9-419667b96e2b', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'n', 'Grade n', 0),
           ('ef279a44-5331-4d3e-a352-997ebdd272f7', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'Q', 'Pending', 0),
           ('513136fb-d07c-4884-9261-c0326a5c2a2c', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'u', 'Unclassified', 0),
           ('decf710c-2d5d-478a-80a5-2f2c60266a18', '11A467CD-FEC7-4A28-85FD-086F2B8982C7', 'X', 'No Result', 0),
           ('bebdcb88-4322-4339-8290-0907957d3ed9', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'AA', 'Grade AA', 240),
           ('ce24a74c-8e23-4d34-bed7-3a9d0fe5baf9', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'AB', 'Grade AB', 220),
           ('0abab08d-be2d-45a0-a66e-b46875c78474', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'BB', 'Grade BB', 200),
           ('2fd2609a-9db9-45f6-8cb6-5fee0a8e7f08', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'BC', 'Grade BC', 180),
           ('58d330f4-41ff-4a1c-be05-e47afd879757', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'CC', 'Grade CC', 160),
           ('0d9a9eea-6bc1-4b8e-b162-eec7c35d59ba', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'CD', 'Grade CD', 140),
           ('44a4c5ec-7c7e-4a9c-b5bb-e0ecb9664d38', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'DD', 'Grade DD', 120),
           ('bbff3473-ff44-44ee-bc39-ed1677861619', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'DE', 'Grade DE', 100),
           ('08441c9c-324a-44a0-bbc3-780a33240bf0', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'EE', 'Grade EE', 80),
           ('847a3b0b-d4cd-4337-89bf-fcdab13a9408', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'EN', 'Grade EN', 2),
           ('12608cec-9df6-4aba-9b72-6dd43889de0f', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'EU', 'Grade NU', 0),
           ('f1ba6516-5b21-453a-a95d-f87940b09ad7', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'NN', 'Grade NN', 0),
           ('bc8cb2e2-36dd-4d00-9a18-861f837c7b6c', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'Q', 'Pending', 0),
           ('8f2dc3cf-0396-421b-b9ae-4f066ab4528c', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'U', 'Unclassified', 0),
           ('be3247a7-11f2-4098-9b63-b64cec93451f', '11A467CD-FEC7-4A28-85FD-086F2B8982C8', 'X', 'No Result', 0),
           ('de87056c-66b8-42f5-9cb7-6cf503d44865', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', '1', 'Level 1', 1),
           ('942e3a33-4559-4b98-b2af-4b4f2aac14f5', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', '2', 'Level 2', 1),
           ('10bf7980-340f-4c40-943d-71c2bb7c4c89', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', '3', 'Level 3', 1),
           ('a30d445a-70db-47d0-adfa-3b189ae28a7e', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', '4', 'Level 4', 1),
           ('600695e2-2d05-4b2a-8091-f097e708cc48', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', '5', 'Level 5', 1),
           ('bf09b91b-d8e8-4259-8590-62bce45f2851', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'Q', 'Pending', 0),
           ('6678c742-e3f1-44d3-b275-12a51bd7ad98', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'U', 'Unclassified', 0),
           ('9b1e1bda-4604-45fe-8941-402f6bb96bb2', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'X', 'No Result', 0),
           ('f0aa4288-5a52-44a8-9abb-e40e0117d33e', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'F', 'Fail', 0),
           ('cd5b7f3f-37c3-468f-8ef8-cf39c40a378d', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'J', 'Within 10%', 0),
           ('f3d16c7e-c734-4877-956c-a64880ecf5cb', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'P', 'Pass', 1),
           ('b1d7237e-66d2-4c68-aaad-ca0adfa30e04', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'Q', 'Pending', 0),
           ('fa35963a-99ce-46c4-97d8-dcc008e7da08', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'U', 'Unclassified', 0),
           ('b56bc95b-3f0c-44fa-8580-06d1cb7f6091', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'V', 'Previously Passed', 0),
           ('7766c99c-73fe-453f-80e3-98b7d7fa0855', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'W', 'Withdrawn', 0),
           ('6a99187a-52ca-4bf2-af46-6a6e9574be96', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'X', 'No Result', 0),
           ('f1528147-00f3-460c-900c-8fe537fdb127', '11A467CD-FEC7-4A28-85FD-086F2B8982CB', 'P', 'Pass', 1),
           ('c1e7c075-25f1-4040-807d-e58d19bbd3e3', '11A467CD-FEC7-4A28-85FD-086F2B8982CB', 'Q', 'Pending', 0),
           ('16d77c53-4a96-4d4c-b577-ad7b370798e3', '11A467CD-FEC7-4A28-85FD-086F2B8982CB', 'U', 'Unclassified', 0),
           ('9400f3ea-5270-4679-b71f-84a29cbcaf61', '11A467CD-FEC7-4A28-85FD-086F2B8982CB', 'X', 'No Result', 0),
           ('7d2fd8fa-f966-4346-8304-67d7779765f7', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', '1', 'Grade 1', 3),
           ('9bc5720f-ac01-4509-81bf-1e3f22907a3d', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', '2', 'Grade 2', 2),
           ('e0159de5-8eaf-4a4a-88aa-29c934d0a79a', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', '3', 'Grade 3', 1),
           ('d346d37c-2f1e-4d97-b68e-992d21580be0', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'Q', 'Pending', 0),
           ('13004bf9-6773-415c-950a-72e798d233a6', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'S', 'Grade S', 4),
           ('6e0956cd-a1b3-4108-9048-3c3841730f96', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'U', 'Unclassified', 0),
           ('485a940d-e45b-4227-bf5e-30bf1f8c2ac1', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'X', 'No Result', 0),
           ('78609277-f37d-44f9-a190-e037d656d950', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'a', 'Grade a', 7),
           ('7f9e86f5-fb19-451e-b1a7-29340b8fcd55', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'a*', 'Grade a*', 8),
           ('fc0a115b-2d46-4def-bc84-9f1c640027ab', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'b', 'Grade b', 6),
           ('a3953b8a-6e6a-4cf4-8714-60d9a1fae715', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'c', 'Grade c', 5),
           ('37632d1b-dccb-4a4d-a4c0-66fbf388dcfb', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'd', 'Grade d', 4),
           ('a64704f9-d8f9-4239-8669-958a694c8196', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'e', 'Grade e', 3),
           ('a9d1052c-d957-4096-9ea8-d1d4706793cc', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'f', 'Grade f', 2),
           ('77e49cda-d710-4559-b900-127259221be6', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'g', 'Grade g', 1),
           ('c8431f0a-9c23-4bcd-8091-2396dbf6e3d2', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'p', 'Grade p', 0),
           ('b438f71a-b92f-4ae1-86a8-fd2c710b0493', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'Q', 'Pending', 0),
           ('f46e1b9d-7ef9-47e8-a948-6bc6497988d9', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'u', 'Unclassified', 0),
           ('ca5f2370-bbd0-4575-b908-a3956bfa845a', '11A467CD-FEC7-4A28-85FD-086F2B8982CD', 'X', 'No Result', 0),
           ('a3abf10d-8eb8-44f7-996e-8d0a6d4bbab8', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'a', 'Grade a', 7),
           ('df680d65-a7ee-4316-976a-3b0dc848809e', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'a*', 'Grade a*', 8),
           ('2ba31f80-1ed1-47c1-819a-b21b9324cae7', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'b', 'Grade b', 6),
           ('203868da-efad-4ccb-851f-3f22c3ceb321', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'c', 'Grade c', 5),
           ('e768078b-c414-4c87-ac62-a24e8a7cf9cb', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'd', 'Grade d', 4),
           ('f669756f-a8a6-47bb-a98f-2cafdf661300', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'e', 'Grade e', 3),
           ('10f2e9a1-7453-4ddd-beb5-96de8966eace', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'f', 'Grade f', 2),
           ('03636991-d3ce-47b7-b7bb-fdb041f76ad8', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'g', 'Grade g', 1),
           ('a491d866-7a8a-437c-90d4-0a4c280dbaf4', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'Q', 'Pending', 0),
           ('99e17c2f-48be-4459-bfb6-3ccaa6f3ecfe', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'u', 'Unclassified', 0),
           ('ff6439c3-6100-48da-a40f-6ed6345597af', '11A467CD-FEC7-4A28-85FD-086F2B8982CE', 'X', 'No Result', 0),
           ('1334138b-0b76-4f2d-a732-29ee3a1b073b', '11A467CD-FEC7-4A28-85FD-086F2B8982CF', 'P', 'Pass', 1),
           ('4d95b645-281d-423d-95f8-c407b67fada9', '11A467CD-FEC7-4A28-85FD-086F2B8982CF', 'Q', 'Pending', 0),
           ('0fa23518-75e6-4c15-ab4d-b006f4c1f21c', '11A467CD-FEC7-4A28-85FD-086F2B8982CF', 'R', 'Refer', 0),
           ('b7fab56b-c0f6-43c7-873e-a78474e471aa', '11A467CD-FEC7-4A28-85FD-086F2B8982CF', 'X', 'No Result', 0),
           ('c5eec779-0815-4b29-bc4e-da7e6b69d3e5', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'D1', 'DDD', 810),
           ('7236c3b2-724b-4dc4-aa19-0e9c4cc318e0', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'D2', 'DDM', 757.5),
           ('d8c275dd-e703-4e85-a909-d6937cdc5ecd', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'D3', 'DMM', 705),
           ('e27d6e24-6dfd-45a3-8a29-168ea38f34a4', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'M1', 'MMM', 652.5),
           ('3adf7a7a-edde-4c56-b7c6-9e76b195f1d1', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'M2', 'MMP', 600),
           ('bb8856d8-b774-4d7d-a726-8ee953fb8062', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'M3', 'MPP', 547.5),
           ('ddf5023d-25ee-4cd9-92e7-9be28cdb8b5e', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'P1', 'PPP', 495),
           ('ee7d00b6-4eed-4a7d-82d2-b2dc16c28990', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'P2', 'PPU', 80),
           ('2f0dab05-7a58-4092-85a7-a1c9973e0f9f', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'P3', 'PUU', 40),
           ('dd8e4640-602d-443f-8312-332abdc76d3f', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'Q', 'Result Pending', 0),
           ('fda7e1b8-6ad7-4dcb-8df8-04aeb5d795b6', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'U', 'UUU', 0),
           ('68a846b8-f492-478f-91f0-ef267de97560', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'X', 'No Result', 0),
           ('20b22c6a-225a-4e5a-a474-87a76ab015f8', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'DD', 'DD', 540),
           ('7e154100-271a-4bb7-b006-102854973b78', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'DM', 'DM', 480),
           ('15ef72de-4398-414b-9863-a2b0b540836c', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'MM', 'MM', 420),
           ('bb178516-70e2-437e-8d38-49d74d9cade6', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'MP', 'MP', 360),
           ('310ad422-9eeb-4100-a5e9-51b8e6055a9c', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'PP', 'PP', 300),
           ('6496ac5d-f8fb-43ae-a38e-ce1ecbda67cb', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'PU', 'PU', 40),
           ('fa9920e5-340c-441f-888f-6c818b192836', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'U', 'UU', 0),
           ('630f4f1d-0e4e-4aff-89fb-0f37ac8dd241', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'X', 'No Result', 0),
           ('fe58a791-088d-4d79-938e-a2d3086943b1', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'C', 'Credit', 2),
           ('a6d01a39-a4c4-4c83-b1d4-e0ca46a75d0b', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'D', 'Distinction', 4),
           ('455e23f6-5c3b-4dec-8067-9b461b77de36', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'M', 'Merit', 3),
           ('265c3d12-9677-4572-ae72-043fababc064', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'P', 'Pass', 1),
           ('a7c99c29-903c-4a9f-a61c-536862de1918', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'Q', 'Pending', 0),
           ('96553a60-3f0c-4925-935e-4222a1004e47', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'U', 'Unclassified', 0),
           ('e6b56399-e44d-42c4-b2cd-b66edd6161fb', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'X', 'No Result', 0),
           ('cf662a63-08a5-4464-9690-afc0504dbea5', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', '1', 'Level 1', 12.5),
           ('0b6a3684-707d-4a95-b606-71d92c38510d', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', '2', 'Level 2', 23),
           ('a068c452-9ac4-422f-bd3b-e863d1ef1f96', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', 'Q', 'Pending', 0),
           ('5f757d43-0123-420f-a16c-0293ccc41fff', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', 'U', 'Unclassified', 0),
           ('d79b406b-ed3f-4fe6-a8e3-1142f7ae7f59', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', 'X', 'No Result', 0),
           ('fb0539ba-c1e6-4642-9c0b-e0728156a89d', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', '1', 'Grade 1', 2.5),
           ('b1fb2d1e-2587-4fc3-922c-13c4c58399b7', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', '2', 'Grade 2', 3),
           ('944b476a-e020-495d-b8a9-1102d0c45703', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', '3', 'Grade 3', 3.5),
           ('b5393f76-739b-4bbf-ac10-c8fe8bb04238', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'Q', 'Pending', 0),
           ('d5c3b013-0ee6-4b42-afb1-2055a2567b60', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'U', 'Unclassified', 0),
           ('c36d5869-d60e-4b0d-ab49-6626b0651f8f', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'X', 'No Result', 0),
           ('a81e47c4-bcbb-4e58-95a5-74f599603b08', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', '4', 'Grade 4', 4.75),
           ('c6818cb0-3e9e-49a7-95a6-12278c0cfbef', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', '5', 'Grade 5', 7),
           ('b6a654f8-7309-4360-ae45-a50d5d180f53', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', '6', 'Grade 6', 8.5),
           ('7d9e47c2-23e4-44d9-b389-5dca221d5e9e', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'Q', 'Pending', 0),
           ('2395c58d-9284-493c-b919-78d59f0775ec', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'U', 'Unclassified', 0),
           ('d6ab6026-19d9-47b6-ad9d-2731ed793083', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'X', 'No Result', 0),
           ('5e4d149d-fee1-47a5-a32f-3e6bdbb69bac', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', '7', 'Grade 7', 10),
           ('1ad4fa67-86af-4bf7-8f7b-35b830c8a28a', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', '8', 'Grade 8', 12.25),
           ('5394364f-c0f0-419c-bf2c-7c82643881f1', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', '9', 'Grade 9', 13.75),
           ('f0be8ab2-f112-4629-ac8e-dd9aab161937', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'Q', 'Pending', 0),
           ('abdc8c3f-92f7-4a09-9282-df66746bc42f', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'U', 'Unclassified', 0),
           ('cd716a62-475d-4b2f-8cf7-8aa0c3461759', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'X', 'No Result', 0),
           ('c8d8f28a-c858-4e2c-b1c1-f40d6289c942', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', '10', 'Grade 10', 10.67),
           ('4c62cb74-ebc9-41a2-bf2d-e5d3c36a2ed0', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', '11', 'Grade 11', 12.25),
           ('49795503-5b20-496c-b285-3b0390e7c151', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', '12', 'Grade 12', 13.83),
           ('b1a26a8c-03d0-4897-99e0-a4dbc789ec83', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', 'Q', 'Pending', 0),
           ('97452627-1fca-417a-91f2-8fb245da9101', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', 'U', 'Unclassified', 0),
           ('f79f1b15-a482-42b5-81f4-7071b4555087', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', 'X', 'No Result', 0)
)
    AS Source (Id, GradeSetId, Code, Description, Value)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Code = Source.Code, Value = Source.Value

WHEN NOT MATCHED THEN
    INSERT (Id, GradeSetId, Code, Value, Description)
    VALUES (Id, GradeSetId, Code, Value, Description);

MERGE INTO [dbo].[ExamQualifications] AS Target
USING (VALUES
           ('49969747-E1AF-47B2-9577-E1D63E7F3572', 'GCE', 'General Certificate of Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3573', 'GCEL', 'General Certificate of Education (Legacy)', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3574', 'VCE', 'Vocational Certificate of Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3575', 'GCSE', 'General Certificate of Secondary Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3576', 'GNVQ', 'General National Vocational Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3577', 'FSMQ', 'Free-Standing Maths Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3578', 'KSKL', 'Key Skills', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3579', 'EL', 'Entry Level', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357A', 'GLU', 'GNVQ Language Unit', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357B', 'AEA', 'Advanced Extension Award', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357C', 'COA', 'Certificate of Achievement', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357D', 'COEA', 'Certificate of Educational Achievement', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357E', 'STEP', 'Sixth Term Examination Papers', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F357F', 'CIAM', 'Certificate in Additional Mathematics', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3580', 'ADSJ', 'Additional Subjects', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3581', 'CAAS', 'Certificate in Arabic and Arab Studies', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3582', 'CJJS', 'Certificate in Japanese and Japanese Studies', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3583', 'FAM', 'Foundations of Advanced Mathematics', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3584', 'BSCT', 'Basic Tests', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3585', 'CFS', 'Certificate of Further Studies', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3586', 'CCES', 'Certificate in Contemporary European Studies', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3587', 'RUS', 'Road User Studies', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3588', 'UET', 'UETESOL', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3589', 'CRP', 'Certificate in Recruitment Practice', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358A', 'DDI', 'Diploma in Driving Instruction', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358B', 'UI', 'Understanding Industry', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358C', 'VRQ', 'Vocationally Related Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358D', 'WBQ', 'Welsh Baccalaureate Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358E', 'LOCL', 'Local', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F358F', 'BTEC', 'BTEC', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3590', 'DIDA', 'Diploma in Digital Applications', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3591', 'ASST', 'Asset', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3592', 'CPWL', 'Certificate in Preparation for Working Life', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3593', 'FSKL', 'Functional Skills', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3594', 'FCSE', 'Foundation Certificate in Secondary Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3595', 'EXPJ', 'Extended Project', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3596', 'L1PJ', 'Level 1 Project', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3597', 'L2PJ', 'Level 2 Project', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3598', 'ONAT', 'OCR Nationals', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F3599', 'ABQ', 'AQA Baccalaureate Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359A', 'ESOL', 'English for Speakers of Other Languages', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359B', 'L1L2', 'Level 1 and Level 2', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359C', 'DPL', 'Diploma', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359D', 'PL', 'Diploma Principal Learning', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359E', 'PREU', 'Pre-University', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F359F', 'GPR', 'Global Perspectives', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A0', 'OCRQ', 'OCR Qualifications', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A1', 'NVQ', 'National Vocational Qualifications', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A2', 'NQF', 'National Qualifications Framework', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A3', 'IB', 'International Baccalaureate', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A4', 'ADIP', 'Adv International Certificate of Education Diploma', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A5', 'AICE', 'Advanced International Certificate of Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A6', 'ESKW', 'Essential Skills (Wales)', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A7', 'ICE', 'International Certificate of Education', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A8', 'BTSC', 'QCF Short Course', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35A9', 'CNAT', 'Cambridge Nationals', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AA', 'CAMT', 'OCR Cambridge Level 2 and Level 3', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AB', 'EDEX', 'Edexcel', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AC', 'WJQ', 'WJEC Qualification', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AD', 'BTNG', 'BTEC Next Generation', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AE', 'PROG', 'OCR Progression', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35AF', 'AW', 'Edexcel Award', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B0', 'L3', 'Level 3', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B1', 'ESNI', 'Essential Skills Northern Ireland', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B2', 'CAMX', 'OCR Cambridge Level 3', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B3', 'OXA', 'Oxford Qualifications', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B4', 'PEA', 'Pearson Assessments', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B5', 'APG', 'Applied General', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B6', 'OXG', 'Oxford International GCSE', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B7', 'TA', 'Technical Awards', 1),
           ('49969747-E1AF-47B2-9577-E1D63E7F35B8', 'AQAA', 'AQA Level 1/2 Award', 1)
)
    AS Source (Id, JcQualificationCode, Description, Active)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, JcQualificationCode = Source.JcQualificationCode, Active = Source.Active

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, JcQualificationCode, System)
    VALUES (Id, Description, Active, JcQualificationCode, 1);

MERGE INTO [dbo].[ExamQualificationLevels] AS Target
USING (VALUES
           ('33b8b836-8e6a-4900-a03b-92e92813c942', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'A', 'GCE Advanced'),
           ('d5185603-d82d-4c08-8710-fd9bebdc1db5', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'ASB', 'GCE Advanced Subsidiary'),
           ('14af5543-8a14-4891-b826-2bb43bc7b419', '49969747-E1AF-47B2-9577-E1D63E7F3572', NULL, 'B', 'GCE Unassigned'),
           ('c1978fa0-846d-41ff-945e-b31f6e56f8d3', '49969747-E1AF-47B2-9577-E1D63E7F3573', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'A', 'GCE A Level'),
           ('9a5ab1b5-e824-4915-9f74-57bc62fe4001', '49969747-E1AF-47B2-9577-E1D63E7F3573', '11A467CD-FEC7-4A28-85FD-086F2B8982C6', 'ASP', 'GCE Advanced Supplementary'),
           ('d5dc0935-c9b6-4f66-bbba-8b9fcfd30b14', '49969747-E1AF-47B2-9577-E1D63E7F3573', NULL, 'B', 'GCE(L) Unassigned '),
           ('7251e4f5-0d77-45e2-857d-b150271bfb10', '49969747-E1AF-47B2-9577-E1D63E7F3574', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'A', 'VCE Advanced'),
           ('2adff6b6-d6c8-4de5-9912-7f313efc4555', '49969747-E1AF-47B2-9577-E1D63E7F3574', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'DA', 'VCE Advanced (Double)'),
           ('08812ba6-5c00-4e9d-b06f-5fe534d9c446', '49969747-E1AF-47B2-9577-E1D63E7F3574', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'ASB', 'VCE Advanced Subsidiary'),
           ('7420ce5a-a96a-4c1a-968d-c24e3f382924', '49969747-E1AF-47B2-9577-E1D63E7F3574', NULL, 'B', 'VCE Unassigned'),
           ('f5ad60ad-5f0b-4b3d-8ce9-7f0697c7c973', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'FC', 'GCSE Full Course'),
           ('86d6a29f-e752-46a1-85c5-9bd6083db6c9', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'SC', 'GCSE Short Course'),
           ('24a624d0-f325-4ed7-a669-7bee1f06e229', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B8982E6', 'DA', 'GCSE Double Award'),
           ('43f2c189-9918-4246-b61e-21dd84a2db88', '49969747-E1AF-47B2-9577-E1D63E7F3575', NULL, 'B', 'GCSE Unassigned'),
           ('a0e767b2-c06d-4436-a0db-9d5dc6c25ef2', '49969747-E1AF-47B2-9577-E1D63E7F3576', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'A', 'GNVQ Advanced'),
           ('804670b6-44b3-463d-a724-8419af50b8bb', '49969747-E1AF-47B2-9577-E1D63E7F3576', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'I', 'GNVQ Intermediate'),
           ('8c78658a-c198-4609-a59b-7f8298a4eaaa', '49969747-E1AF-47B2-9577-E1D63E7F3576', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'P1I', 'GNVQ Intermediate Part 1'),
           ('6297c8da-c598-4f0a-92f9-1cc2c60b8d4c', '49969747-E1AF-47B2-9577-E1D63E7F3576', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'F', 'GNVQ Foundation'),
           ('070bb55f-3f78-4189-a8ca-d6e66d159245', '49969747-E1AF-47B2-9577-E1D63E7F3576', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'P1F', 'GNVQ Foundation Part 1'),
           ('f4df8715-8bff-47f0-8127-657ce6a5d687', '49969747-E1AF-47B2-9577-E1D63E7F3576', NULL, 'B', 'GNVQ Unassigned'),
           ('c759c51f-f6ef-4b86-b652-958635f7e176', '49969747-E1AF-47B2-9577-E1D63E7F3577', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'A', 'FSMQ Advanced'),
           ('8d15a26f-ae13-492d-ba3f-25d4639d1fb3', '49969747-E1AF-47B2-9577-E1D63E7F3577', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'I', 'FSMQ Intermediate'),
           ('14ec45d1-2bee-4d1f-aa85-b6a9da163778', '49969747-E1AF-47B2-9577-E1D63E7F3577', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'F', 'FSMQ Foundation'),
           ('15a23c52-f106-4c5a-a62a-da622fd8dddd', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L1', 'Key Skills Level 1'),
           ('98858ecf-7031-4ab2-a507-2951a71a1122', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L2', 'Key Skills Level 2'),
           ('44fc5f03-db15-4b45-9405-b6a14554e0f2', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L3', 'Key Skills Level 3'),
           ('0efa6544-a7ca-4b2a-b7f5-2e61f6b81230', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L4', 'Key Skills Level 4'),
           ('cc1468f1-7630-46c7-94c0-920078259e17', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L5', 'Key Skills Level 5'),
           ('94c7c509-a7bb-49a9-b7a2-f32653464331', '49969747-E1AF-47B2-9577-E1D63E7F3578', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'B', 'Key Skills Unassigned'),
           ('670a5a70-15d5-41e7-9a5e-07ce90ed2814', '49969747-E1AF-47B2-9577-E1D63E7F3579', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'B', 'EL Unassigned'),
           ('b5c14766-e15d-4d4c-a848-af1f8a73e07f', '49969747-E1AF-47B2-9577-E1D63E7F357A', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'B', 'GLU Unassigned'),
           ('28f78269-6515-4b91-b928-1db4ca4b0038', '49969747-E1AF-47B2-9577-E1D63E7F357B', '11A467CD-FEC7-4A28-85FD-086F2B8982C5', 'B', 'AEA Unassigned'),
           ('eead133b-832c-4461-84a7-01d868e77ff5', '49969747-E1AF-47B2-9577-E1D63E7F357C', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'B', 'COA Unassigned'),
           ('cbe07499-978a-451b-ace6-53b6762956fa', '49969747-E1AF-47B2-9577-E1D63E7F357D', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'FC', 'COEA Full Course'),
           ('4380ab39-c390-42bc-be39-4d8bfa690cb3', '49969747-E1AF-47B2-9577-E1D63E7F357D', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'B', 'COEA Unassigned'),
           ('1f51958d-3599-4d7b-8db9-e47aa9a2455a', '49969747-E1AF-47B2-9577-E1D63E7F357E', '11A467CD-FEC7-4A28-85FD-086F2B8982CC', 'B', 'STEP Unassigned'),
           ('fdf2e186-e76d-496d-a870-3d919aeffc67', '49969747-E1AF-47B2-9577-E1D63E7F357F', NULL, 'B', 'CIAM Unassigned'),
           ('45d86002-fbcb-4791-970c-030755529e6c', '49969747-E1AF-47B2-9577-E1D63E7F3580', NULL, 'B', 'ADSJ Unassigned'),
           ('77d1d73b-a9eb-4d23-8885-a01895097f07', '49969747-E1AF-47B2-9577-E1D63E7F3581', NULL, 'B', 'CAAS Unassigned'),
           ('7e07c09b-5244-473d-9f3a-3fb5943b0386', '49969747-E1AF-47B2-9577-E1D63E7F3582', NULL, 'B', 'CJJS Unassigned'),
           ('b8680998-86ff-479e-9fc7-95dcfc407413', '49969747-E1AF-47B2-9577-E1D63E7F3583', NULL, 'B', 'FAM Unassigned'),
           ('a7208b19-e13e-4054-a217-089b40071c54', '49969747-E1AF-47B2-9577-E1D63E7F3584', NULL, 'B', 'BSCT Unassigned'),
           ('7fcb29ce-bba9-4ce1-9888-d9462da1573f', '49969747-E1AF-47B2-9577-E1D63E7F3585', NULL, 'B', 'CFS Unassigned'),
           ('a6cc032d-ca88-46cd-9d4d-01c8a339a7da', '49969747-E1AF-47B2-9577-E1D63E7F3586', NULL, 'B', 'CCES Unassigned'),
           ('5f0bf1d1-b0c9-4cb5-8934-444ccc1f48b1', '49969747-E1AF-47B2-9577-E1D63E7F3587', NULL, 'B', 'RUS Unassigned'),
           ('7cad7ef3-0e95-4448-9c76-49e7a7e5057f', '49969747-E1AF-47B2-9577-E1D63E7F3588', NULL, 'B', 'UET Unassigned'),
           ('139aee03-b194-4997-ad64-934918d4ee7d', '49969747-E1AF-47B2-9577-E1D63E7F3589', NULL, 'B', 'CRP Unassigned'),
           ('35592a76-087f-4ede-8e1f-934f63d8f7fa', '49969747-E1AF-47B2-9577-E1D63E7F358A', NULL, 'B', 'DDI Unassigned'),
           ('823707e9-cbaf-445d-999b-2e2af2124ea0', '49969747-E1AF-47B2-9577-E1D63E7F358B', NULL, 'B', 'UI Unassigned'),
           ('e3fa1186-4c82-4a6c-a02e-cf0e74ebd5a5', '49969747-E1AF-47B2-9577-E1D63E7F3579', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'FC', 'EL Full Course'),
           ('55039f5b-4b5b-4317-994a-6b65ffa3989c', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'I', 'VRQ Intermediate'),
           ('84e6324b-d55e-40aa-b103-7fd9ea8ccf0d', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'A', 'VRQ Advanced'),
           ('7608b1e6-7c75-407a-b006-170c1ad40349', '49969747-E1AF-47B2-9577-E1D63E7F357A', NULL, 'I', 'GLU Intermediate'),
           ('2922783d-d9e7-42c6-86e4-d5fa8c448c4f', '49969747-E1AF-47B2-9577-E1D63E7F357A', NULL, 'F', 'GLU Foundation'),
           ('705adf7c-33ae-4ec4-bcd6-18a3083a4964', '49969747-E1AF-47B2-9577-E1D63E7F358D', NULL, 'A', 'WBQ Advanced'),
           ('064f9bd2-d057-46ed-bd6b-f1574f0a3d71', '49969747-E1AF-47B2-9577-E1D63E7F358D', NULL, 'I', 'WBQ Intermediate'),
           ('4aec4de3-ef8d-4d54-8a61-d14ac9b4944c', '49969747-E1AF-47B2-9577-E1D63E7F358D', NULL, 'F', 'WBQ Foundation'),
           ('272945f2-79c3-4113-b40f-da7d2df4e065', '49969747-E1AF-47B2-9577-E1D63E7F358E', NULL, 'B', 'Unassigned'),
           ('9a382dc9-7269-48a2-97dc-44c515b3c137', '49969747-E1AF-47B2-9577-E1D63E7F358E', NULL, 'KS3', 'Key Stage 3'),
           ('83aaee61-a5f7-46ab-9581-398f08b4f5a4', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'ICE', 'BTEC Introductory Certificate'),
           ('fa48b9e6-048e-46f0-a5d6-aea4df48769c', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'IDI', 'BTEC Introductory Diploma'),
           ('ef73677d-3bb5-414d-937d-3c72c99dd66c', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'FCE', 'BTEC First Certificate'),
           ('171c8d48-4143-4d72-a423-834cdb30b40e', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'FDI', 'BTEC First Diploma'),
           ('d4f33080-385f-4b04-952f-16fbdeb4b579', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982D1', 'NCE', 'BTEC National Certificate'),
           ('d74db0ec-4c8d-4e83-95a2-aa806d7e69bf', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982D0', 'NDI', 'BTEC National Diploma'),
           ('5f4a340c-6c4f-451a-b2d6-012b1ad74c41', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'NA', 'BTEC National Award'),
           ('0373a415-8f56-4ab9-a070-15df34b07c27', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'AW1', 'DiDA Award Level 1'),
           ('97c05055-f894-49e8-9596-e829f86ff5c5', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A467CD-FEC7-4A28-85FD-086F2B8982D2', 'AW2', 'DiDA Award Level 2'),
           ('01ee4eb3-ee64-4678-a43e-82af1692d40f', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'CE1', 'DiDA Certificate Level 1'),
           ('a9135661-5730-4058-a6f9-8ee09ebabd9c', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'CE2', 'DiDA Certificate Level 2'),
           ('85f2c842-c330-4175-beb9-4e7b30081cf7', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'DI1', 'DiDA Diploma Level 1'),
           ('319f2bbd-a2d0-440c-8ad7-de4d406ff73d', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'DI2', 'DiDA Diploma Level 2'),
           ('35f022e5-0ff2-4b06-ae80-d9442d259a14', '49969747-E1AF-47B2-9577-E1D63E7F3590', '11A469CD-FEC7-4A28-85FD-086F2B8982E0', 'B', 'DiDA Unassigned'),
           ('5dc4cf74-bd19-4286-aa86-5ba1cb083702', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A469CD-FEC7-4A28-85FD-086F2B8982E1', 'DA', 'GCE A Double Award'),
           ('05e4081e-3a15-4802-98db-d8c748a98759', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A467CD-FEC7-4A28-85FD-086F2B8982C0', 'ASD', 'GCE ASB Double Award'),
           ('3d7faeab-0db6-40f3-865c-ca09badc035d', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW1', 'BTEC Award Level 1'),
           ('9075875f-7883-4a54-8d41-12781d16c927', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW2', 'BTEC Award Level 2'),
           ('5c4ffb1a-c235-43b3-bf97-16820c9f79a1', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW3', 'BTEC Award Level 3'),
           ('555c11a1-7ec0-4d04-a566-311fb322e00b', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'AW4', 'BTEC Award Level 4'),
           ('e38507df-3399-4be0-865a-3d39c54db4a7', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'CE1', 'BTEC Certificate Level 1'),
           ('d0e2dec5-d683-4d11-b3c4-1d9e653c8508', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'CE2', 'BTEC Certificate Level 2'),
           ('65108dac-3d28-4b4f-826b-44b92dedfc22', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'CE3', 'BTEC Certificate Level 3'),
           ('ee77a8bf-a4eb-4356-a429-5c7e65934c6e', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'CE4', 'BTEC Certificate Level 4'),
           ('78dbbfba-afa2-4e9f-b63e-dfbcdeecde64', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'DI1', 'BTEC Diploma Level 1'),
           ('035e0e03-9b34-4936-8264-498790c590a8', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'DI2', 'BTEC Diploma Level 2'),
           ('57bfeb54-b4a4-495f-9714-def554cf0325', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982E9', 'DI3', 'BTEC Diploma Level 3'),
           ('c3356c04-927f-47ca-8aa9-a57ee1c1c9e6', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'DI4', 'BTEC Diploma Level 4'),
           ('4d595c20-4761-46e1-b742-c24cab440b9f', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'FC', 'BTEC Full Course'),
           ('5d54c3c7-854b-4f41-9309-466218b4aa77', '49969747-E1AF-47B2-9577-E1D63E7F3591', NULL, 'PRO', 'Asset Proficiency'),
           ('0bb1c3c7-1610-4c9f-83e3-2a5ffdfdddac', '49969747-E1AF-47B2-9577-E1D63E7F3591', NULL, 'MAS', 'Asset Mastery'),
           ('6b24c796-3efc-4718-b700-5b0ca42c5a7f', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'B', 'FSKL Unassigned'),
           ('338b7642-f180-4f5e-8f28-4c3f3e5146a6', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'L1', 'FSKL Level 1'),
           ('73fcd44c-e9a1-48e9-b5e3-9e4ed90761f4', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'L2', 'FSKL Level 2'),
           ('ffaa67ea-b261-450b-bbb6-011e46c6a6f4', '49969747-E1AF-47B2-9577-E1D63E7F3594', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'B', 'FCSE Unassigned'),
           ('3b411750-e5ae-48c6-b82f-2de021ca28b3', '49969747-E1AF-47B2-9577-E1D63E7F3595', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'B', 'Ext Project Unassigned'),
           ('85502f1c-49eb-42bb-8c98-d962eed51e6c', '49969747-E1AF-47B2-9577-E1D63E7F3596', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'B', 'L1 Project Unassigned'),
           ('eca7887a-be2d-4dfe-9725-aca3a0f567e0', '49969747-E1AF-47B2-9577-E1D63E7F3597', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'B', 'L2 Project Unassigned'),
           ('bbb7de9a-2128-4247-849b-46efc67305f3', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'FA1', 'ONAT First Award Level 1'),
           ('56c09ffc-908c-4e30-a98b-b6f399a69842', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW1', 'ONAT Award Level 1'),
           ('c406b346-017a-4dec-888e-1c372c957f94', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW2', 'ONAT Award Level 2'),
           ('54adaef1-ea2e-464c-9870-e3b3d06ac53b', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'FC2', 'ONAT First Cert Level 2'),
           ('b07a226f-3b6e-44a6-bb6b-f1233a00919e', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'CE1', 'ONAT Certificate Level 1'),
           ('eb604e52-181d-44f5-a136-77663add17c8', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'CE2', 'ONAT Certificate Level 2'),
           ('b3d5f1fc-0179-4493-aa17-8a432390b09b', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'CE3', 'ONAT Certificate Level 3'),
           ('d3249402-feba-432e-ba9f-599f08c80ace', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'DI3', 'ONAT Diploma Level 3'),
           ('22b2e510-cc60-4f95-aad4-cd7cdff61d18', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'ED3', 'ONAT Extended Diploma'),
           ('c4cac69b-cd26-4a52-bb0b-7e515f6f92fe', '49969747-E1AF-47B2-9577-E1D63E7F3598', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'FA2', 'ONAT First Award Level 2'),
           ('9ace0162-5a03-4985-a0b4-a9b06d78a1c9', '49969747-E1AF-47B2-9577-E1D63E7F3599', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'B', 'ABQ Unassigned'),
           ('40f67f14-7aa2-48aa-b0e0-46818ed89ebf', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A469CD-FEC7-4A28-85FD-086F2B8982E2', 'AAS', 'GCE Advanced + Subsidiary'),
           ('d8240d9d-2dbd-4e1d-b563-8eb7c64c2133', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A469CD-FEC7-4A28-85FD-086F2B8982E4', 'B', 'ESOL Unassigned'),
           ('839eb972-e445-4805-9c99-c006132249e1', '49969747-E1AF-47B2-9577-E1D63E7F359B', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', 'B', 'L1L2 Unassigned'),
           ('5637229b-fcab-4e21-95c5-67b3c44de0e2', '49969747-E1AF-47B2-9577-E1D63E7F359C', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'B', 'Diploma Unassigned'),
           ('abed0323-f610-4947-8c64-bbf06c238d44', '49969747-E1AF-47B2-9577-E1D63E7F359C', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'L1', 'Diploma Level 1'),
           ('c3746e82-222f-4177-ad3d-2a2e0ef86fa0', '49969747-E1AF-47B2-9577-E1D63E7F359C', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'L2', 'Diploma Level 2'),
           ('4735889e-ec3f-4a6c-87f4-4607c5485ed9', '49969747-E1AF-47B2-9577-E1D63E7F359C', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'L3', 'Diploma Level 3'),
           ('3f3fc007-62ac-4cdf-a7e7-51c8d9011f54', '49969747-E1AF-47B2-9577-E1D63E7F359D', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'B', 'Diploma PL Unassigned'),
           ('2c1f583f-2b99-4755-bd85-60e513fd95be', '49969747-E1AF-47B2-9577-E1D63E7F359D', '11A469CD-FEC7-4A28-85FD-086F2B8982DD', 'L1', 'Diploma PL Level 1'),
           ('4a32c849-0f7f-4118-97ce-1afcc5be1f49', '49969747-E1AF-47B2-9577-E1D63E7F359D', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'L2', 'Diploma PL Level 2'),
           ('19f35897-0798-4720-9e06-c3709cfd4079', '49969747-E1AF-47B2-9577-E1D63E7F359D', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'L3', 'Diploma PL Level 3'),
           ('51b363c8-a890-4dc2-a438-410d31417ba5', '49969747-E1AF-47B2-9577-E1D63E7F359E', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'B', 'PREU Unassigned'),
           ('1383302c-8d3d-4166-be9f-26d4b4265938', '49969747-E1AF-47B2-9577-E1D63E7F359F', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'B', 'GPR Unassigned'),
           ('678ce055-d752-4d2c-b7f6-6863a122a9e3', '49969747-E1AF-47B2-9577-E1D63E7F35A0', NULL, 'L2', 'OCRQ Level 2'),
           ('77c858ac-9e41-4c14-831e-be5f194e48a4', '49969747-E1AF-47B2-9577-E1D63E7F35A0', NULL, 'L3', 'OCRQ Level 3'),
           ('eeac18b6-12af-4468-bcd9-d843551ee0d4', '49969747-E1AF-47B2-9577-E1D63E7F35A1', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L1', 'NVQ Level 1'),
           ('58e0c949-9987-40e8-9531-0b2b19a9c6e3', '49969747-E1AF-47B2-9577-E1D63E7F35A1', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L2', 'NVQ Level 2'),
           ('a3599fd7-862a-4275-adc7-bd447e560bd2', '49969747-E1AF-47B2-9577-E1D63E7F35A1', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L3', 'NVQ Level 3'),
           ('a827351d-921a-4801-8d40-50876e7d35d1', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'BRK', 'Asset Breakthrough'),
           ('86db1b55-66d1-46ff-b084-907ea4ec3f4c', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'PRE', 'Asset Preliminary'),
           ('51495f9f-a9ac-4951-bbaf-a152b1b263ae', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'INT', 'Asset Intermediate'),
           ('7d556d61-45c6-4e76-b988-86ba1f7fe68b', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D7', 'ADV', 'Asset Advanced'),
           ('c8037121-cf68-4b0a-9708-da641296f973', '49969747-E1AF-47B2-9577-E1D63E7F3592', '11A467CD-FEC7-4A28-85FD-086F2B8982D3', 'B', 'CPWL Unassigned'),
           ('a5e82a9d-c2e5-4cf0-9096-2c8f6717689d', '49969747-E1AF-47B2-9577-E1D63E7F35A2', NULL, 'L1', 'NQF Level 1'),
           ('a9861031-f733-49ae-9a2f-29eaa06237f1', '49969747-E1AF-47B2-9577-E1D63E7F35A2', NULL, 'L2', 'NQF Level 2'),
           ('635e27bb-e188-459f-b9e2-5a638690caa6', '49969747-E1AF-47B2-9577-E1D63E7F35A2', NULL, 'L3', 'NQF Level 3'),
           ('ab3ab137-ca50-49f7-9918-2b3dab3a116b', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L1', 'VRQ Level 1'),
           ('8411c491-f3ec-4efb-8317-8e9a1cd179d0', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L2', 'VRQ Level 2'),
           ('4af59255-01eb-49aa-ae11-879e89a3d774', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L3', 'VRQ Level 3'),
           ('05bf6dcd-8da1-457b-873f-2c85939136c6', '49969747-E1AF-47B2-9577-E1D63E7F35A3', NULL, 'B', 'IB Unassigned'),
           ('458bee4b-a27a-4f74-a309-4c03205d8919', '49969747-E1AF-47B2-9577-E1D63E7F3594', NULL, 'FC', 'FCSE Full Course'),
           ('4ca732ea-4cb1-4faf-a102-cfd2dfc1db60', '49969747-E1AF-47B2-9577-E1D63E7F3594', NULL, 'SC', 'FCSE Short Course'),
           ('66ad414e-164c-421b-ac7b-b69fd23eccaf', '49969747-E1AF-47B2-9577-E1D63E7F35A4', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'DI', 'ADIP Diploma'),
           ('821d5fba-b8f7-4ddf-bd64-64700aa0de1f', '49969747-E1AF-47B2-9577-E1D63E7F35A5', NULL, 'HC', 'AICE Half Credit'),
           ('8e5aef46-0a08-4193-a434-3f54bfff1bea', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'EL1', 'ESKW Entry Level 1'),
           ('0a5561df-350c-4865-bb63-80d2334781b3', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'EL2', 'ESKW Entry Level 2'),
           ('c6280596-3410-4f93-975a-7ae32e030efe', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'EL3', 'ESKW Entry Level 3'),
           ('74312a8e-c14e-49cf-b7e6-87115c968eb4', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L1', 'ESKW Level 1'),
           ('747faf40-8c4e-43f4-933a-5b8e5d7db08a', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L2', 'ESKW Level 2'),
           ('ccca6d69-b796-46ea-a9ed-afd1e153fb55', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L3', 'ESKW Level 3'),
           ('37cb5b6e-da9a-4066-9bb2-20c83bce7d39', '49969747-E1AF-47B2-9577-E1D63E7F35A6', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L4', 'ESKW Level 4'),
           ('affc327f-658f-42be-b3b3-0983e0af7458', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'EC1', 'BTEC Extended Certificate Level 1'),
           ('7a2cf3df-8a2a-4bc6-801d-a6572c0b3dec', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'EC2', 'BTEC Extended Certificate Level 2'),
           ('52554f79-885d-464e-bb79-497b4e7fd344', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'EC3', 'BTEC Extended Certificate Level 3'),
           ('62eae780-818b-4620-8f33-3291c986c6e8', '49969747-E1AF-47B2-9577-E1D63E7F35A7', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'CE', 'ICE Certificate'),
           ('339361e2-cd08-417f-a576-5771bd9e3f0a', '49969747-E1AF-47B2-9577-E1D63E7F3572', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'O', 'GCE Ordinary'),
           ('63615a93-bcab-49e0-aa27-103617578576', '49969747-E1AF-47B2-9577-E1D63E7F359E', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'FC', 'PREU Full Course'),
           ('248bc70b-ed8c-4c73-acfa-9bddbe656349', '49969747-E1AF-47B2-9577-E1D63E7F359E', '11A469CD-FEC7-4A28-85FD-086F2B8982E3', 'SC', 'PREU Short Course'),
           ('4f3b3dfc-749d-4ce6-ba97-65448d597eec', '49969747-E1AF-47B2-9577-E1D63E7F359F', '11A469CD-FEC7-4A28-85FD-086F2B8982E5', 'FC', 'Global Perspectives Full Course'),
           ('720d6429-6799-482f-aacd-9b1b3094bd2f', '49969747-E1AF-47B2-9577-E1D63E7F359C', NULL, 'P', 'Diploma Level P'),
           ('7b958c57-6582-49f8-b82d-54696d7cae55', '49969747-E1AF-47B2-9577-E1D63E7F3590', NULL, 'EC1', 'DIDA Extended Certificate Level 1'),
           ('23a61b9a-d073-499c-8836-1595cc45d4e3', '49969747-E1AF-47B2-9577-E1D63E7F3590', NULL, 'EC2', 'DIDA Extended Certificate Level 2'),
           ('a8dd4742-8261-4732-b935-5803f74d1a82', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'DI1', 'ONAT Diploma Level 1'),
           ('b27b090c-cf99-4060-a2e2-5d9db403d8c7', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'DI2', 'ONAT Diploma Level 2'),
           ('39013c54-51b9-4abf-be54-4f8170d7fe70', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'ED1', 'ONAT Extended Diploma Level 1'),
           ('aab04ebf-4a4c-41b4-8197-8f09a3beaa4e', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'ED2', 'ONAT Extended Diploma Level 2'),
           ('7a48b83c-1179-48da-a2a1-109a9c0642c3', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'L1', 'ONAT Level 1'),
           ('1a0815aa-94a9-403f-8e06-19292cfa324d', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'L2', 'ONAT Level 2'),
           ('21be5ef5-2f67-46e5-bdb8-f37405bc5700', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'L3', 'ONAT Level 3'),
           ('b1f2a60d-e0d8-4088-9c99-95e6ac5e6dac', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'ICE', 'ONAT Introductory Certificate'),
           ('61805f09-4ee4-4034-88c2-29593f116faa', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'IDI', 'ONAT Introductory Diploma'),
           ('8a3d1dbf-2a56-4020-ba10-76bf49995241', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'FC', 'ESOL Full Course'),
           ('4229292b-f07d-4bc9-b2e4-11504b15c80f', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'SC', 'ESOL Short Course'),
           ('3ba3c850-9a24-49ea-835d-7865abf9688e', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'AWE', 'BTSC Entry Level Award'),
           ('fd486e1e-2489-4462-b3cc-16e6a07cbe67', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'CEE', 'BTSC Entry Level Certificate'),
           ('207a1592-8c62-4a13-8918-d986917017c9', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'DIE', 'BTSC Entry Level Diploma'),
           ('16e2612d-44d2-4efa-bd3d-220f105feff0', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SA1', 'BTSC Level 1 Subsidiary Award'),
           ('98882896-96c8-4287-9621-c0e8ed27c1e7', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'AW1', 'BTSC Level 1 Award'),
           ('fef076aa-5b09-42f5-a7a2-4ae2619d10d8', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EA1', 'BTSC Level 1 Extended Award'),
           ('df2e79fc-b38d-499f-8bb6-8b5d16913830', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SC1', 'BTSC Level 1 Subsidiary Certificate'),
           ('5f61ce67-744b-4e33-90f7-8bc824666c90', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'CE1', 'BTSC Level 1 Certificate'),
           ('dbefdfdb-90b8-49e5-9d65-4ed9b24c3d8f', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EC1', 'BTSC Level 1 Extended Certificate'),
           ('d1826442-6f43-4223-addd-9163c2f578e6', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SD1', 'BTSC Level 1 Subsidiary Diploma'),
           ('86874944-2029-4bb1-9c25-0b8732a976b7', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'DI1', 'BTSC Level 1 Diploma'),
           ('3667c82a-be22-4072-b6ca-2941dafa01d9', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'ED1', 'BTSC Level 1 Extended Diploma'),
           ('a7dc6ada-df89-45e4-92e4-de01c6edba85', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SA2', 'BTSC Level 2 Subsidiary Award'),
           ('e9e97a32-25d3-458e-b410-17f49c1545e9', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'AW2', 'BTSC Level 2 Award'),
           ('16de6ab6-44e2-4607-bc61-536cc8f73909', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EA2', 'BTSC Level 2 Extended Award'),
           ('14f8ed81-ff6f-40d0-ac62-252171540a0d', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SC2', 'BTSC Level 2 Subsidiary Certificate'),
           ('824260e7-6530-4ef3-9019-38f6e9ea8d69', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'CE2', 'BTSC Level 2 Certificate'),
           ('b42a6548-dc49-4324-a04b-c5ff83745147', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EC2', 'BTSC Level 2 Extended Certificate'),
           ('d2a1d012-6d03-44c8-b956-dd1e8f2552d8', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SD2', 'BTSC Level 2 Subsidiary Diploma'),
           ('de201add-aa1e-466d-ba62-d21ea7351a47', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'DI2', 'BTSC Level 2 Diploma'),
           ('6c1af4d8-d6c6-434c-b6d7-4eddb294137b', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'ED2', 'BTSC Level 2 Extended Diploma'),
           ('6f0a630f-b35b-4d53-8398-fab6dc82cab5', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SA3', 'BTSC Level 3 Subsidiary Award'),
           ('c5175fbf-e37f-48e5-9afb-98fe98a92c1d', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'AW3', 'BTSC Level 3 Award'),
           ('5a6823f1-b36d-4d60-8700-e8caa5ce6073', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EA3', 'BTSC Level 3 Extended Award'),
           ('98ddcc49-d159-438d-ad05-3b35e34363ce', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SC3', 'BTSC Level 3 Subsidiary Certificate'),
           ('16c03478-4e7a-4176-84fa-e0b3e3d9f5c7', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'CE3', 'BTSC Level 3 Certificate'),
           ('975fce35-2f4f-45fb-8238-55cc08426794', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'EC3', 'BTSC Level 3 Extended Certificate'),
           ('389323dd-4206-490a-a361-e80ae28e31b1', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'SD3', 'BTSC Level 3 Subsidiary Diploma'),
           ('b6d89b27-929d-4b9c-a45f-fa81607417aa', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'DI3', 'BTSC Level 3 Diploma'),
           ('0858989b-0d94-4026-a9cd-02cc56d679f2', '49969747-E1AF-47B2-9577-E1D63E7F35A8', NULL, 'ED3', 'BTSC Level 3 Extended Diploma'),
           ('63f8db7f-1460-4b11-b916-d0afbcc299b5', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L4', 'VRQ Level 4'),
           ('f4df6bfc-a888-4ba7-8ff8-0ff1f29fa7eb', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L5', 'VRQ Level 5'),
           ('43f8e974-8475-4f1d-b8cc-f5e1c2862543', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'L6', 'VRQ Level 6'),
           ('44c50508-6354-4447-b0c8-54ac42046583', '49969747-E1AF-47B2-9577-E1D63E7F35A0', NULL, 'FC', 'OCRQ Full Course'),
           ('f5011250-12a8-4a05-adc3-c79020c809e1', '49969747-E1AF-47B2-9577-E1D63E7F3577', NULL, 'H', 'FSMQ Higher'),
           ('aa0412e9-1747-4735-ab6c-7d32bbaa0204', '49969747-E1AF-47B2-9577-E1D63E7F3579', NULL, 'AW', 'EL Award'),
           ('54ee7e79-669c-4b10-86f4-785d519d142d', '49969747-E1AF-47B2-9577-E1D63E7F3579', NULL, 'CE', 'EL Certificate'),
           ('622a148f-e948-4619-8b06-2d42541c8087', '49969747-E1AF-47B2-9577-E1D63E7F3579', NULL, 'DI', 'EL Diploma'),
           ('907af2e1-a790-41d6-9cdc-cd6e632a6c75', '49969747-E1AF-47B2-9577-E1D63E7F35A9', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', '1&2', 'CNAT Level 1/2'),
           ('dbcef9c8-fe8e-4d05-9d58-f6b218b9719f', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'L2', 'CAMT Level Two'),
           ('6c69f94b-999d-4bc4-b15d-a9217a69c74d', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'CE2', 'CAMT Certificate Level 2'),
           ('7fb8e8de-5d9e-4e1d-8164-a4bff42d9c65', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'EC2', 'CAMT Extended Certificate Level 2'),
           ('bfa35773-b7bd-49c9-b57d-8867d53746b0', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'DI2', 'CAMT Diploma Level 2'),
           ('76ecec64-1ff1-4477-b36e-422c92a64a41', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982E7', 'L3', 'CAMT Level Three'),
           ('173f95e1-d43d-4181-b72b-e33b64267459', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'CE3', 'CAMT Certificate Level 3'),
           ('f97564eb-e114-433b-b50c-9dc8600d4300', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'SD3', 'CAMT Level 3 Subsidiary Diploma'),
           ('7f9ce3d8-82db-494c-b6e3-a6935f5cfc9d', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EF', 'DI3', 'CAMT Diploma Level 3'),
           ('7f636648-ab6c-4bbb-b6b4-c464c8ad7c89', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982F0', 'ED3', 'CAMT Extended Diploma Level 3'),
           ('25c23ddf-744c-4202-b5ec-1687812be046', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982F3', '1&2', 'BTEC Level 1/2'),
           ('22c2bc3b-2e61-4722-8106-461eed5c2f2a', '49969747-E1AF-47B2-9577-E1D63E7F35AB', '11A469CD-FEC7-4A28-85FD-086F2B8982F1', 'KS2', 'EDEX Key Stage 2'),
           ('ef5ee06d-6f33-477c-8c47-3e9506af1047', '49969747-E1AF-47B2-9577-E1D63E7F35AB', '11A469CD-FEC7-4A28-85FD-086F2B8982F2', 'KS3', 'EDEX Key Stage 3'),
           ('ea948125-6709-4aa5-b941-f36b330b9c8c', '49969747-E1AF-47B2-9577-E1D63E7F35A0', NULL, 'B', 'OCRQ Unassigned'),
           ('538d005f-fbca-4b29-a4d7-8abd0c5a13a5', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'AWE', 'BTEC Entry Level Award'),
           ('94f96c69-6273-439b-a61c-8fcde5ab6bf0', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SA1', 'BTEC Level 1 Subsidiary Award '),
           ('6bc4fed1-05a0-4331-a345-f823f51acbc4', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'EA1', 'BTEC Level 1 Extended Award '),
           ('79114410-42e5-4144-95b4-079476200df9', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SC1', 'BTEC Level 1 Subsidiary Certificate'),
           ('43de6c4e-2bf6-4b73-94f4-42ac05de1056', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SD1', 'BTEC Level 1 Subsidiary Diploma'),
           ('265aa7ae-7eb6-4b28-8e24-4fcbac8aa9ea', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'ED1', 'BTEC Level 1 Extended Diploma'),
           ('96910fed-fb0c-4308-a90d-371c4c8014df', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'EA2', 'BTEC Level 2 Extended Award'),
           ('bb411110-08c4-4dad-97b3-b9df691791a8', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SC2', 'BTEC Level 2 Subsidiary Certificate'),
           ('c2a820ba-1b98-4712-88ab-42c4b0d25cec', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SD2', 'BTEC Level 2 Subsidiary Diploma'),
           ('08ffb915-42da-4abe-b7fa-e0fdded30a0e', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'ED2', 'BTEC Level 2 Extended Diploma'),
           ('ff6921f9-7c72-4365-84f8-01d982ceb3cc', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SA3', 'BTEC Level 3 Subsidiary Award'),
           ('957f82bd-f919-49b0-b7c8-0f2848293cbd', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'EA3', 'BTEC Level 3 Extended Award'),
           ('768085b4-d66e-4683-8e7a-6acf77d37325', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'SC3', 'BTEC Level 3 Subsidiary Certificate'),
           ('f4741c90-a81f-49ce-94f8-953f7bcc9fc4', '49969747-E1AF-47B2-9577-E1D63E7F359B', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'L1', 'L1L2 Level 1'),
           ('742fd103-36af-43c8-8720-088dd5476bf0', '49969747-E1AF-47B2-9577-E1D63E7F359B', '11A469CD-FEC7-4A28-85FD-086F2B8982DF', 'L2', 'L1L2 Level 2'),
           ('fdf5fe6f-4093-4d53-95ae-08065cc9529f', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'ID3', 'CAMT Level 3 Introductory Diploma'),
           ('3fca9c29-4d13-4d8e-a7db-c14b4f6043e0', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'EL3', 'WJQ Entry Level 3'),
           ('ba2c0869-9e0f-4aae-b9b7-8f9a18539ea8', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'L1', 'WJQ Level 1'),
           ('6d0df5b6-320f-4188-9704-811bfc7a9762', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'L2', 'WJQ Level 2'),
           ('36f1f7de-e0ad-44a5-88c3-dbb52369a392', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'L3', 'WJQ Level 3'),
           ('4f7c6783-8357-490f-9836-546004b73a77', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'EL', 'FSKL Entry Level '),
           ('42409a97-4b8f-4267-bee4-7e6c2184b49f', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'EL1', 'FSKL Entry Level 1'),
           ('131344ac-b53f-42c2-8129-31f1d674960a', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'EL2', 'FSKL Entry Level 2'),
           ('0181b963-e79c-48e7-a708-bc5427e3a603', '49969747-E1AF-47B2-9577-E1D63E7F3593', '11A467CD-FEC7-4A28-85FD-086F2B8982C9', 'EL3', 'FSKL Entry Level 3'),
           ('36b33402-9ac7-4753-b571-2ca86239d8e9', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982EA', 'ED3', 'BTEC Extended Diploma Level 3'),
           ('273361df-5e11-4829-a3ed-474a01a9516e', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'SD3', 'BTEC Subsidiary Diploma Level 3'),
           ('46543cdd-79de-4f8d-a65a-05f4f02ba6ac', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'DIE', 'BTEC Entry Level Diploma'),
           ('dad8336e-63b8-4859-ac6d-1f0eaf26bc58', '49969747-E1AF-47B2-9577-E1D63E7F35A2', NULL, 'E', 'Entry Level'),
           ('365a7bf5-b7f0-44a8-a989-7d0861508543', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'A12', 'BTNG L1/L2 First Award'),
           ('956b9b97-e5a6-4741-a530-07cc0079c218', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'C12', 'BTNG L1/L2 First Certificate'),
           ('6587672a-9ed0-4e5d-91ca-f508a6b206ac', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'E12', 'BTNG L1/L2 First Extended Certificate'),
           ('8b0d4466-5ea5-4365-b540-6551c5632221', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'D12', 'BTNG L1/L2 First Diploma'),
           ('005c613c-c218-4f9c-8ebc-ed4aab834720', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'B', 'BTNG Unassigned'),
           ('812aec26-5023-4aaa-a735-d4a164916bee', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'SA3', 'BTNG National Subsidiary Award'),
           ('f51d8e1a-775a-4a83-bf9f-216f89aa8ae3', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'AW3', 'BTNG National Award'),
           ('f9e6f3a6-0b29-459e-8571-cb73be9e0db2', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'SC3', 'BTNG National Subsidiary Certificate'),
           ('1b6c314b-8dbd-4265-9c12-13e2533c3754', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'CE3', 'BTNG National Certificate'),
           ('c3caf2d1-37e5-4fca-a751-5c9f4fddfdef', '49969747-E1AF-47B2-9577-E1D63E7F35AD', NULL, 'DI3', 'BTNG National Diploma'),
           ('dcbf8028-a3e2-49b9-8f42-93a37e38a058', '49969747-E1AF-47B2-9577-E1D63E7F35AE', NULL, 'EL1', 'PROG Entry Level 1'),
           ('36e4e771-dd69-4d4d-b5ea-91bd2b621670', '49969747-E1AF-47B2-9577-E1D63E7F35AE', NULL, 'EL2', 'PROG Entry Level 2'),
           ('1c3552ef-88f8-4202-b531-abe156eab612', '49969747-E1AF-47B2-9577-E1D63E7F35AE', NULL, 'EL3', 'PROG Entry Level 3'),
           ('b6cdd3aa-dc6b-465d-9012-e4fcace3bfe5', '49969747-E1AF-47B2-9577-E1D63E7F35AE', NULL, 'L1', 'PROG Level 1'),
           ('c7f949fa-9780-4383-b84b-818154c63cdf', '49969747-E1AF-47B2-9577-E1D63E7F35AE', NULL, 'L2', 'PROG Level 2'),
           ('4433d43f-c0aa-4dc9-9f90-bb24fc2afdb3', '49969747-E1AF-47B2-9577-E1D63E7F35AF', NULL, 'L1', 'Edexcel Award Level 1'),
           ('547a0a17-48fb-4f92-8cc2-194f1cf4fb5a', '49969747-E1AF-47B2-9577-E1D63E7F35AF', NULL, 'L2', 'Edexcel Award Level 2'),
           ('bfcb8df7-d896-4092-bf39-b6dd9853c120', '49969747-E1AF-47B2-9577-E1D63E7F35AF', NULL, 'L3', 'Edexcel Award Level 3'),
           ('d2339d24-48b0-4f69-810f-5410ce05aad5', '49969747-E1AF-47B2-9577-E1D63E7F359B', NULL, 'SC', 'L1L2 Short Course'),
           ('18a716f4-1107-4de7-a716-3b53973c90ae', '49969747-E1AF-47B2-9577-E1D63E7F359B', '11A467CD-FEC7-4A28-85FD-086F2B8982C3', 'FC', 'L1L2 Full Course'),
           ('a97657c9-6d51-49ff-b67a-9ae38715962b', '49969747-E1AF-47B2-9577-E1D63E7F35B0', NULL, 'B', 'L3 Unassigned'),
           ('8d7504d3-75db-421c-b29a-7971486f2658', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D4', 'EL', 'ASST Breakthrough'),
           ('9b6f6039-b03c-4df4-80a5-414d5bed6965', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D5', 'L1', 'ASST Preliminary'),
           ('45c3646a-dad7-45b1-8682-1dbe7c8fcec6', '49969747-E1AF-47B2-9577-E1D63E7F3591', '11A467CD-FEC7-4A28-85FD-086F2B8982D6', 'L2', 'ASST Intermediate'),
           ('717940ba-f091-4817-a498-59645ddc48b0', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'L4', 'CAMT Level Four'),
           ('65e248ce-7d42-4c59-85b4-d0430dae7556', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'DI4', 'CAMT Diploma Level 4'),
           ('acb2a8f0-750f-4465-837c-eef3e13f1c88', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'AE1', 'ESOL Award Entry Level 1'),
           ('0c1143e7-6f1e-47f3-b990-0df15c38b9e0', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'AE2', 'ESOL Award Entry Level 2'),
           ('f41ddbae-3ef1-48cc-be3d-aa54765e6339', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'AE3', 'ESOL Award Entry Level 3'),
           ('cc6ab49d-3aad-483f-b8d6-377e8c74dec5', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'CE1', 'ESOL Certificate Entry level 1'),
           ('e3c63135-e333-485d-b292-cd9eb521cfe1', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'CE2', 'ESOL Certificate Entry level 2'),
           ('463acd57-fe1e-46da-b206-d9401dc12533', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'CE3', 'ESOL Certificate Entry level 3'),
           ('9733f5be-dcfd-4672-9b97-0bea4641b357', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'AW1', 'ESOL Award Level 1'),
           ('c7bffccc-be5b-4a85-80d9-481b51b9dab4', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'AW2', 'ESOL Award Level 2'),
           ('4543b936-08db-4939-95ec-278a4adfae20', '49969747-E1AF-47B2-9577-E1D63E7F35B1', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'CE1', 'ESNI Certificate Level 1'),
           ('554ae967-874a-4277-8384-a627d229350d', '49969747-E1AF-47B2-9577-E1D63E7F35B1', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'CE2', 'ESNI Certificate Level 2'),
           ('9366aca9-e6c4-4080-b012-6f20bf1216fe', '49969747-E1AF-47B2-9577-E1D63E7F35AB', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'B', 'Skilled For Life/Traineeship'),
           ('d783f814-d4f6-4582-84f6-c7c447afc5c6', '49969747-E1AF-47B2-9577-E1D63E7F359A', '11A467CD-FEC7-4A28-85FD-086F2B8982C4', 'EL', 'ESOL Entry Level'),
           ('28dfa259-4aab-4e51-9e11-ee41aeb1e12c', '49969747-E1AF-47B2-9577-E1D63E7F35AB', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'CE3', 'EDEX Certificate Level 3'),
           ('b2335aa0-a4fc-4b09-9a8d-c6df0b202af3', '49969747-E1AF-47B2-9577-E1D63E7F35A0', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'L4', 'OCRQ Level 4'),
           ('2f949e45-e24d-41f5-ba5d-c02895c58224', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'L3', 'CAMX Level Three'),
           ('d0b7b2ab-c54f-493f-8510-3c3c2fc3fcd5', '49969747-E1AF-47B2-9577-E1D63E7F35B2', NULL, 'CE3', 'CAMX Certificate Level 3'),
           ('d190f3b0-2cbe-42cc-856b-85f241ebd87a', '49969747-E1AF-47B2-9577-E1D63E7F35B2', NULL, 'EC3', 'CAMX Extended Certificate Level 3'),
           ('e5121932-fbe1-4f5c-a669-4d8fd758128f', '49969747-E1AF-47B2-9577-E1D63E7F35B2', NULL, 'FD', 'CAMX Foundation Diploma Level 3'),
           ('8b010c6d-9c3a-4609-b3ed-dd0283182f11', '49969747-E1AF-47B2-9577-E1D63E7F35B2', NULL, 'DI3', 'CAMX Diploma Level 3'),
           ('49f7872a-e0d9-4574-8521-15e8818691c0', '49969747-E1AF-47B2-9577-E1D63E7F358D', NULL, '1&2', 'WBQ Level 1/2'),
           ('aeb93c45-1ffa-4b2e-9ac0-156ae20fb7cf', '49969747-E1AF-47B2-9577-E1D63E7F359E', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'DI', 'PREU Diploma'),
           ('e516d61a-62a9-4d40-a845-058f2ad0aebc', '49969747-E1AF-47B2-9577-E1D63E7F35B0', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'L3', 'Level Three'),
           ('6c4eb816-7a76-4fdd-b3c0-e0a32a16cc09', '49969747-E1AF-47B2-9577-E1D63E7F35B3', NULL, 'B', 'OXA Unassigned'),
           ('87e7e652-7d14-4e8e-9e45-301e61059733', '49969747-E1AF-47B2-9577-E1D63E7F35B3', '11A469CD-FEC7-4A28-85FD-086F2B8982DB', 'A', 'OXA Advanced'),
           ('0f6040b8-641a-4535-ac82-676198a5456a', '49969747-E1AF-47B2-9577-E1D63E7F35B3', '11A467CD-FEC7-4A28-85FD-086F2B8982BE', 'ASB', 'OXA Advanced Subsidiary'),
           ('72432374-f432-4f6c-8333-0091bfb30d17', '49969747-E1AF-47B2-9577-E1D63E7F35B4', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'FC', 'PEA Full Course'),
           ('835a9895-0130-4f33-bcba-6112b9a3dab2', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A469CD-FEC7-4A28-85FD-086F2B8982F9', 'ID3', 'CAMX Level 3 Introductory Diploma'),
           ('8f21fb15-971f-4411-ab99-60af680c8742', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A469CD-FEC7-4A28-85FD-086F2B8982FA', 'ED3', 'CAMX Extended Diploma Level 3'),
           ('6141d9ba-336f-4b92-8ac8-e7d908722565', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982DA', 'FD3', 'BTEC Foundation Diploma Level 3'),
           ('e7b88956-e2c6-4277-8161-0cceafcd4340', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'EC3', 'CAMT Extended Certificate Level 3'),
           ('d663ea4f-21f0-437f-8e00-e69c1d6b64b1', '49969747-E1AF-47B2-9577-E1D63E7F35AA', '11A469CD-FEC7-4A28-85FD-086F2B8982EE', 'FD3', 'CAMT Foundation Diploma Level 3'),
           ('7baea4db-5e7d-402c-b79a-92badbdbc8ba', '49969747-E1AF-47B2-9577-E1D63E7F35B5', '11A469CD-FEC7-4A28-85FD-086F2B898311', 'B', 'Applied General Unit'),
           ('96cdec33-c131-4d4f-a872-9599e8ac4c83', '49969747-E1AF-47B2-9577-E1D63E7F35B5', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'CE', 'Applied General Certificate'),
           ('2d82e8fb-d192-4db8-8ed9-6f4d32fa1a8a', '49969747-E1AF-47B2-9577-E1D63E7F35B5', '11A469CD-FEC7-4A28-85FD-086F2B898312', 'EC', 'Applied General Extended Certificate'),
           ('45b258e0-45cb-4e3d-a9a6-e4a0e6dcfeb4', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '9FC', 'GCSE (9-1) Full Course'),
           ('b85ae7c5-dd26-4547-b74d-40c2dc93c2ef', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B898300', '9DA', 'GCSE (9-1) Full Course (Double Award)'),
           ('c339e857-e3f2-4009-ac53-47d97c0a3526', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B898301', '9ED', 'GCSE endorsed grade (9-1)'),
           ('9ab13500-6a14-4fb0-a4fc-70125fdb683d', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'CEE', 'BTEC Entry Level Certificate'),
           ('07513022-a6c7-49fe-b361-2e1e9d419790', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', '9SC', 'GCSE (9-1) Short Course'),
           ('da8f0f0f-9c6d-48bd-97a5-4e146b3b7931', '49969747-E1AF-47B2-9577-E1D63E7F35B6', '11A469CD-FEC7-4A28-85FD-086F2B8982FF', 'FC', 'OXG Full Course'),
           ('e6b4f111-efde-4320-8401-49eb50976090', '49969747-E1AF-47B2-9577-E1D63E7F35B6', '11A469CD-FEC7-4A28-85FD-086F2B898318', 'DA', 'OXG Double Award'),
           ('037dd456-bff2-40e1-93c3-d3a0cdc03bec', '49969747-E1AF-47B2-9577-E1D63E7F35B7', NULL, 'B', 'TA Unassigned'),
           ('d6ae146c-77aa-4ccf-a54e-a3086985fd36', '49969747-E1AF-47B2-9577-E1D63E7F35B8', NULL, 'B', 'AQAA Unassigned'),
           ('c237b2fb-66f9-4ad2-af8f-d471f3053110', '49969747-E1AF-47B2-9577-E1D63E7F358F', NULL, 'B', 'BTEC Unassigned'),
           ('66542c71-6fa5-4717-8142-95ffc1203eff', '49969747-E1AF-47B2-9577-E1D63E7F358F', '11A469CD-FEC7-4A28-85FD-086F2B8982EC', 'A12', 'BTEC Tech Award Level 1 & 2'),
           ('fcf3772b-0a80-4215-94b4-7c4aa1d08356', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A469CD-FEC7-4A28-85FD-086F2B89831D', 'L2', 'CAMX Level Two'),
           ('b3b99844-0539-4c0e-ba66-454ca98d4711', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A467CD-FEC7-4A28-85FD-086F2B8982C1', 'AW2', 'CAMX Award Level 2'),
           ('60693f23-8c4a-4ff0-8038-86d6baee2aa6', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'CE2', 'CAMX Certificate Level 2'),
           ('b11ba3ef-ef7a-4445-95bc-1a0fc2debdab', '49969747-E1AF-47B2-9577-E1D63E7F35B2', '11A469CD-FEC7-4A28-85FD-086F2B8982F8', 'DI2', 'CAMX Diploma Level 2'),
           ('64dcfa36-7f2a-4f30-bc6a-8862e1da7f2e', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'FC*', 'GCSE (C*) Full Course'),
           ('229d0991-6269-430b-8f79-7acd032f9454', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B898303', 'SC*', 'GCSE (C*) Short Course'),
           ('c37244de-2759-47bd-82bd-5dd9f12555f7', '49969747-E1AF-47B2-9577-E1D63E7F3575', '11A469CD-FEC7-4A28-85FD-086F2B898315', 'DA*', 'GCSE (C*) Full Course (Double Award)'),
           ('6cbfff71-aae5-47c2-bb20-9d1237e7c03b', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'AW3', 'WJQ Level 3 Award'),
           ('32407402-ea33-4ddb-b2da-0278a7900005', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'CE3', 'WJQ Level 3 Certificate'),
           ('4dbfdada-5954-40a0-b26d-afcd73de049f', '49969747-E1AF-47B2-9577-E1D63E7F35AC', NULL, 'DI3', 'WJQ Level 3 Diploma'),
           ('6b94a100-e78b-4ee8-8600-6e70be409924', '49969747-E1AF-47B2-9577-E1D63E7F358C', NULL, 'EL3', 'VRQ Entry Level 3'),
           ('fb19bd51-dc57-4bde-b995-b23f2d6696c2', '49969747-E1AF-47B2-9577-E1D63E7F3598', NULL, 'SH2', 'ONAT Short Course Level 2'),
           ('51948ce3-933f-46e9-990c-774a03ba5a11', '49969747-E1AF-47B2-9577-E1D63E7F3591', NULL, 'IND', 'Intermediate Diploma'),
           ('0bf2893e-7e22-42b7-a17e-27d5e06929b3', '49969747-E1AF-47B2-9577-E1D63E7F358D', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L1', 'WBQ Level 1'),
           ('65055fc9-2e8c-4c3e-bae7-6d3cee31b51e', '49969747-E1AF-47B2-9577-E1D63E7F358D', '11A467CD-FEC7-4A28-85FD-086F2B8982CA', 'L2', 'WBQ Level 2'),
           ('4b0b82f1-f778-49b7-b476-67abfebdc8a3', '49969747-E1AF-47B2-9577-E1D63E7F359E', '11A469CD-FEC7-4A28-85FD-086F2B8982E8', 'DI3', 'PREU Diploma Level 3')

)
    AS Source (Id, QualificationId, DefaultGradeSetId, JcLevelCode, Description)
ON Target.Id = Source.Id

WHEN MATCHED THEN UPDATE SET DefaultGradeSetId = Source.DefaultGradeSetId

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, QualificationId, DefaultGradeSetId, JcLevelCode, System)
    VALUES (Id, Description, 1, QualificationId, DefaultGradeSetId, JcLevelCode, 1);

MERGE INTO [dbo].[AttendanceCodeTypes] AS Target
USING (VALUES
           ('59036717-D349-46D3-B8A6-60FFA9263DB3', 'Present'),
           ('59036718-D349-46D3-B8A6-60FFA9263DB3', 'Authorised Absence'),
           ('59036719-D349-46D3-B8A6-60FFA9263DB3', 'Approved Educational Activity'),
           ('59036720-D349-46D3-B8A6-60FFA9263DB3', 'Unauthorised Absence'),
           ('59036721-D349-46D3-B8A6-60FFA9263DB3', 'Attendance not Required'),
           ('59036722-D349-46D3-B8A6-60FFA9263DB3', 'No Mark')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Description)
    VALUES (Id, Description);

MERGE INTO [dbo].[AttendanceCodes] AS Target
USING (VALUES
           ('EBACEBAB-153B-452E-B2F4-9CFF11D1B083', '/', 'Present (AM)', '59036717-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-163B-452E-B2F4-9CFF11D1B083', '\', 'Present (PM)', '59036717-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-173B-452E-B2F4-9CFF11D1B083', 'B', 'Off site educational activity', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-183B-452E-B2F4-9CFF11D1B083', 'C', 'Other authorised circumstance', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-193B-452E-B2F4-9CFF11D1B083', 'D', 'Dual registration', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-1A3B-452E-B2F4-9CFF11D1B083', 'E', 'Excluded', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-1C3B-452E-B2F4-9CFF11D1B083', 'G', 'Family holiday (not agreed)', '59036720-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-1D3B-452E-B2F4-9CFF11D1B083', 'H', 'Family holiday (agreed)', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-1E3B-452E-B2F4-9CFF11D1B083', 'I', 'Illness', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-1F3B-452E-B2F4-9CFF11D1B083', 'J', 'Interview', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-203B-452E-B2F4-9CFF11D1B083', 'L', 'Late (before registers closed)', '59036717-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-213B-452E-B2F4-9CFF11D1B083', 'M', 'Medical/dental appointment', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-223B-452E-B2F4-9CFF11D1B083', 'N', 'No reason yet provided for absence', '59036720-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-233B-452E-B2F4-9CFF11D1B083', 'O', 'Unauthorised absence', '59036720-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-243B-452E-B2F4-9CFF11D1B083', 'P', 'Approved sporting activity', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-253B-452E-B2F4-9CFF11D1B083', 'R', 'Religious observance', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-263B-452E-B2F4-9CFF11D1B083', 'S', 'Study leave', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-273B-452E-B2F4-9CFF11D1B083', 'T', 'Traveller absence', '59036718-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-283B-452E-B2F4-9CFF11D1B083', 'U', 'Late (after registers closed)', '59036720-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-293B-452E-B2F4-9CFF11D1B083', 'V', 'Educational visit or trip', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-2A3B-452E-B2F4-9CFF11D1B083', 'W', 'Work experience', '59036719-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-2B3B-452E-B2F4-9CFF11D1B083', 'X', 'Non-compulsory school age absence', '59036721-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-2C3B-452E-B2F4-9CFF11D1B083', 'Y', 'Unable to attend due to exceptional circumstance', '59036721-D349-46D3-B8A6-60FFA9263DB3', 1, 1),
           ('EBACEBAB-2D3B-452E-B2F4-9CFF11D1B083', 'Z', 'Pupil not on roll', '59036721-D349-46D3-B8A6-60FFA9263DB3', 0, 1),
           ('EBACEBAB-2E3B-452E-B2F4-9CFF11D1B083', '#', 'Planned whole or partial school closure', '59036721-D349-46D3-B8A6-60FFA9263DB3', 0, 1)
)
    AS Source (Id, Code, Description, AttendanceCodeTypeId, Active, Restricted)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Code = Source.Code, Description = Source.Description, AttendanceCodeTypeId = Source.AttendanceCodeTypeId

WHEN NOT MATCHED THEN
    INSERT (Id, Code, Description, AttendanceCodeTypeId, Active, Restricted, System)
    VALUES (Id, Code, Description, AttendanceCodeTypeId, Active, 0, 1);

MERGE INTO [dbo].[AchievementOutcomes] AS Target
USING (VALUES
           ('1017B966-E589-4B87-A21F-B85081EE958B', 'Commended', 1),
           ('1017B967-E589-4B87-A21F-B85081EE958B', 'Certificate', 1),
           ('1017B968-E589-4B87-A21F-B85081EE958B', 'Merit Mark', 1),
           ('1017B969-E589-4B87-A21F-B85081EE958B', 'Merit', 1),
           ('1017B96A-E589-4B87-A21F-B85081EE958B', '8 Merit Letter', 1),
           ('1017B96B-E589-4B87-A21F-B85081EE958B', 'Not Applicable', 1),
           ('1017B96C-E589-4B87-A21F-B85081EE958B', 'Letter Sent to Parent or Guardian', 1),
           ('1017B96D-E589-4B87-A21F-B85081EE958B', 'Bronze Certificate', 1),
           ('1017B96E-E589-4B87-A21F-B85081EE958B', 'Positive Referral', 1),
           ('1017B96F-E589-4B87-A21F-B85081EE958B', 'Prize', 1),
           ('1017B970-E589-4B87-A21F-B85081EE958B', 'Gold Certificate', 1),
           ('1017B971-E589-4B87-A21F-B85081EE958B', 'Platinum Certificate', 1),
           ('1017B972-E589-4B87-A21F-B85081EE958B', 'Trophy', 1),
           ('1017B973-E589-4B87-A21F-B85081EE958B', 'Letter of Commendation', 1),
           ('1017B974-E589-4B87-A21F-B85081EE958B', 'Prefect Badge', 1),
           ('1017B975-E589-4B87-A21F-B85081EE958B', 'Emergency Aid Certificate', 1),
           ('1017B976-E589-4B87-A21F-B85081EE958B', 'Reading Certificate', 1),
           ('1017B977-E589-4B87-A21F-B85081EE958B', 'Sports Certificate', 1),
           ('1017B978-E589-4B87-A21F-B85081EE958B', 'College Colours', 1),
           ('1017B979-E589-4B87-A21F-B85081EE958B', 'Prize-winner', 1),
           ('1017B97A-E589-4B87-A21F-B85081EE958B', 'Community Certificate', 1),
           ('1017B97B-E589-4B87-A21F-B85081EE958B', 'Lifestyle Award Winner', 1),
           ('1017B97C-E589-4B87-A21F-B85081EE958B', 'Subject Referral', 1),
           ('1017B97D-E589-4B87-A21F-B85081EE958B', 'Other', 1)
)

    AS Source(Id, Description, Active)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = Source.Active

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[AchievementTypes] AS Target
USING (VALUES
           ('F4BD5F85-CDD7-4937-A346-EAAF1A173CD5', 'Academic', 1),
           ('F4BD5F85-CDD7-4938-A346-EAAF1A173CD5', 'Choir', 1),
           ('F4BD5F85-CDD7-4939-A346-EAAF1A173CD5', 'Club', 1),
           ('F4BD5F85-CDD7-493A-A346-EAAF1A173CD5', 'Excellent Effort', 1),
           ('F4BD5F85-CDD7-493B-A346-EAAF1A173CD5', 'Other', 1),
           ('F4BD5F85-CDD7-493C-A346-EAAF1A173CD5', 'Outstanding Work', 1),
           ('F4BD5F85-CDD7-493D-A346-EAAF1A173CD5', 'Prefect', 1),
           ('F4BD5F85-CDD7-493E-A346-EAAF1A173CD5', 'School Band', 1),
           ('F4BD5F85-CDD7-493F-A346-EAAF1A173CD5', 'School Orchestra', 1),
           ('F4BD5F85-CDD7-4940-A346-EAAF1A173CD5', 'Sporting Representation', 1),
           ('F4BD5F85-CDD7-4941-A346-EAAF1A173CD5', 'Visit to School', 1)
)
    AS Source(Id, Description, DefaultPoints)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DefaultPoints = Source.DefaultPoints

WHEN NOT MATCHED THEN
    INSERT (Id, Description, DefaultPoints, Active)
    VALUES (Id, Description, DefaultPoints, 1);

MERGE INTO [dbo].[BehaviourOutcomes] AS Target
USING (VALUES
           ('59C4E8DB-CFEF-462B-855A-9092897E7135', 'Counselling', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7136', 'Cooling off Period', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7137', 'Detention', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7138', 'Daily Report', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7139', 'Detention Given - Pastoral', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713A', 'Fixed Period Exclusion', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713B', 'Excluded from School', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713C', 'Detention Given - Faculty', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713D', 'Discussed with Pupil', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713E', 'Permanent Exclusion', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E713F', 'Reinstated from Fixed Period', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7140', 'Isolation', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7141', 'Letter to Parent/Guardian', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7142', 'Reinstated from Permanent', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7143', 'Governors Sub-Committee', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7144', 'Not Applicable', 1, 1),
           ('59C4E8DB-CFEF-462B-855A-9092897E7145', 'Parents/Guardian Informed', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7146', 'Letter to Parent/Guardian', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7147', 'Referred to Leadership Group', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7148', 'Reprimand Given', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7149', 'Refer to Deputy Head', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714A', 'Refer to Form Tutor', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714B', 'Truancy Report', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714C', 'Written Punishment', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714D', 'Refer to Head Teacher', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714E', 'Refer to Head of Year', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E714F', 'Suspended from School', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7150', 'Actions Agreed', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7151', 'Additional Internal Support', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7152', 'Discussed with Aggressor', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7153', 'Discussed with Other Pupils', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7154', 'Discussed with Parents', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7155', 'Discussion of Incident with Peers', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7156', 'Discussed with Bully/Pupil/Student Target', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7157', 'Further Intervention Required', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7158', 'Follow Up Date Set', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E7159', 'Internal Exclusion', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E715A', 'Involved Outside Agency', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E715B', 'Other', 1, 1),
           ('59C4E8DB-CFEF-462B-855A-9092897E715C', 'Report on File', 1, 0),
           ('59C4E8DB-CFEF-462B-855A-9092897E715D', 'Sanctions Applied in line with School''s Behaviour Policy', 1, 0)
)

    AS Source(Id, Description, Active, System)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = Source.Active, System = Source.System

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, System)
    VALUES (Id, Description, Active, System);

MERGE INTO [dbo].[BehaviourStatus] AS Target
USING (VALUES
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD434', 'Unresolved', 1, 0),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD435', 'Resolved', 1, 1),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD436', 'Further Intervention Required', 1, 0),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD437', 'Review in 1 Week', 1, 0),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD438', 'Review in 2 Weeks', 1, 0),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD439', 'Review in 3 Weeks', 1, 0),
           ('997FBFDD-DD28-4F2C-9336-2858A5FBD43A', 'Review in 6 Weeks', 1, 0)
)

    AS Source(Id, Description, Active, Resolved)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = Source.Active, Resolved = Source.Resolved

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, Resolved)
    VALUES (Id, Description, Active, Resolved);

MERGE INTO [dbo].[IncidentTypes] AS Target
USING (VALUES
           ('1EDD7DFC-13B8-4565-9AF7-88E20E99C43B', 'Accident', 1, 1),
           ('1EDD7DFC-13B8-4665-9AF7-88E20E99C43B', 'Assault - Pupil', 1, 1),
           ('1EDD7DFC-13B8-4765-9AF7-88E20E99C43B', 'Bullying', 1, 1),
           ('1EDD7DFC-13B8-4865-9AF7-88E20E99C43B', 'Code of Conduct Not Completed', 1, 1),
           ('1EDD7DFC-13B8-4965-9AF7-88E20E99C43B', 'Damage - Property', 1, 1),
           ('1EDD7DFC-13B8-4A65-9AF7-88E20E99C43B', 'Defiance', 1, 1),
           ('1EDD7DFC-13B8-4B65-9AF7-88E20E99C43B', 'Disruption', 1, 1),
           ('1EDD7DFC-13B8-4C65-9AF7-88E20E99C43B', 'Equipment', 1, 1),
           ('1EDD7DFC-13B8-4D65-9AF7-88E20E99C43B', 'Fighting', 1, 1),
           ('1EDD7DFC-13B8-4E65-9AF7-88E20E99C43B', 'Homework', 1, 1),
           ('1EDD7DFC-13B8-4F65-9AF7-88E20E99C43B', 'Illicit Substances', 1, 1),
           ('1EDD7DFC-13B8-5065-9AF7-88E20E99C43B', 'Inadequate Work', 1, 1),
           ('1EDD7DFC-13B8-5165-9AF7-88E20E99C43B', 'Insolence', 1, 1),
           ('1EDD7DFC-13B8-5265-9AF7-88E20E99C43B', 'Missed Detention', 1, 1),
           ('1EDD7DFC-13B8-5365-9AF7-88E20E99C43B', 'Not Applicable', 1, 1),
           ('1EDD7DFC-13B8-5465-9AF7-88E20E99C43B', 'Other (Minor)', 1, 1),
           ('1EDD7DFC-13B8-5565-9AF7-88E20E99C43B', 'Other (Severe)', 3, 1),
           ('1EDD7DFC-13B8-5665-9AF7-88E20E99C43B', 'Persistent Homework Failure', 1, 1),
           ('1EDD7DFC-13B8-5765-9AF7-88E20E99C43B', 'Persistent Lateness to Lessons', 1, 1),
           ('1EDD7DFC-13B8-5865-9AF7-88E20E99C43B', 'Persistent Lateness to School', 1, 1),
           ('1EDD7DFC-13B8-5965-9AF7-88E20E99C43B', 'Racist Incident', 1, 1),
           ('1EDD7DFC-13B8-5A65-9AF7-88E20E99C43B', 'Smoking', 1, 1),
           ('1EDD7DFC-13B8-5B65-9AF7-88E20E99C43B', 'Theft', 1, 1),
           ('1EDD7DFC-13B8-5C65-9AF7-88E20E99C43B', 'Truancy', 1, 1),
           ('1EDD7DFC-13B8-5D65-9AF7-88E20E99C43B', 'Uniform/Jewellery etc', 1, 1),
           ('1EDD7DFC-13B8-5E65-9AF7-88E20E99C43B', 'Verbal Abuse', 1, 1)
)
    AS Source(Id, Description, DefaultPoints, Active)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET DefaultPoints = Source.DefaultPoints, Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Description, DefaultPoints, Active)
    VALUES (Id, Description, DefaultPoints, Active);

MERGE INTO [dbo].[CommentTypes] AS Target
USING (VALUES
           ('57DEAF3C-1E3F-4D44-A516-15A79A1DC18C', 'Heading', 1, 1),
           ('57DEAF3C-1E3F-4D44-A516-15A79A1DC18D', 'Join', 1, 1),
           ('57DEAF3C-1E3F-4D44-A516-15A79A1DC18E', 'New Line', 1, 1)
)
    AS Source(Id, Description, DefaultPoints, Active)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[CommunicationTypes] AS Target
USING (VALUES
           ('08890B44-AD75-4753-8DE8-BB5F60FFBB79', 'Email'),
           ('08890B45-AD75-4753-8DE8-BB5F60FFBB79', 'Telephone'),
           ('08890B46-AD75-4753-8DE8-BB5F60FFBB79', 'Fax'),
           ('08890B49-AD75-4753-8DE8-BB5F60FFBB79', 'Letter'),
           ('08890B50-AD75-4753-8DE8-BB5F60FFBB79', 'SMS Message')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[AddressTypes] AS Target
USING (VALUES
           ('B9B02CB0-8333-4EE7-995C-4D1FE88DCEA5', 'Home'),
           ('B9B02CB0-8333-4EE7-995C-4D1FE88DCEA6', 'Work')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[EmailAddressTypes] AS Target
USING (VALUES
           ('52FC17D3-EBB3-4DCD-A06D-6EB5A7EF2782', 'Home'),
           ('52FC17D3-EBB4-4DCD-A06D-6EB5A7EF2782', 'Work'),
           ('52FC17D3-EBB5-4DCD-A06D-6EB5A7EF2782', 'Other')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[PhoneNumberTypes] AS Target
USING (VALUES
           ('4BE15DCD-8F53-4AB9-933E-4E586B6FBF6E', 'Mobile'),
           ('4BE25DCD-8F53-4AB9-933E-4E586B6FBF6E', 'Home'),
           ('4BE35DCD-8F53-4AB9-933E-4E586B6FBF6E', 'Work'),
           ('4BE45DCD-8F53-4AB9-933E-4E586B6FBF6E', 'Fax')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[DocumentTypes] AS Target
USING (VALUES


           ('5D1555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 0, 1, 'Individualised Education Plan'),
           ('5D2555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 0, 1, 'SEN Review'),
           ('5D3555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 1, 0, 0, 0, 'Employment Contract'),
           ('5D4555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 1, 0, 'Newsletter'),
           ('5D5555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 1, 0, 'Meeting Minutes'),
           ('5D6555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 0, 0, 'Lesson Plan Attachment'),
           ('5D7555DE-0C38-4FCC-BB54-C3C4A7E81201', 1, 1, 1, 1, 1, 'Other'),
           ('5D8555DE-0C38-4FCC-BB54-C3C4A7E81201', 0, 0, 0, 0, 0, 'Homework Attachment')
)
    AS Source (Id, Student, Staff, Contact, General, Sen, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Student, Staff, Contact, General, Sen, Active, System)
    VALUES (Id, Description, Student, Staff, Contact, General, Sen, 1, 1);

MERGE INTO [dbo].[ProductTypes] AS Target
USING (VALUES
           ('9508DB1A-1FC2-44E2-9BB7-E0924A003376', 'Meals', 1),
           ('9509DB1A-1FC2-44E2-9BB7-E0924A003376', 'Trips', 0),
           ('950ADB1A-1FC2-44E2-9BB7-E0924A003376', 'Uniform', 0),
           ('950BDB1A-1FC2-44E2-9BB7-E0924A003376', 'Stationery', 0),
           ('950CDB1A-1FC2-44E2-9BB7-E0924A003376', 'Books', 0)
)
    AS Source (Id, Description, IsMeal)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET IsMeal = Source.IsMeal, Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Description, IsMeal, Active)
    VALUES (Id, Description, IsMeal, 1);

MERGE INTO [dbo].[MedicalConditions] AS Target
USING (VALUES
           ('EF9AD33E-CE89-45F8-B66C-F7F28828F4D4', 'Epilepsy', 1),
           ('EF9AD33E-CE8A-45F8-B66C-F7F28828F4D4', 'Diabetes', 1),
           ('EF9AD33E-CE8B-45F8-B66C-F7F28828F4D4', 'Asthma', 1),
           ('EF9AD33E-CE8C-45F8-B66C-F7F28828F4D4', 'Eczema', 1),
           ('EF9AD33E-CE8D-45F8-B66C-F7F28828F4D4', 'Arthritis', 1),
           ('EF9AD33E-CE8E-45F8-B66C-F7F28828F4D4', 'Multiple Sclerosis', 1),
           ('EF9AD33E-CE8F-45F8-B66C-F7F28828F4D4', 'Tuberculosis', 1),
           ('EF9AD33E-CE90-45F8-B66C-F7F28828F4D4', 'HIV', 1),
           ('EF9AD33E-CE91-45F8-B66C-F7F28828F4D4', 'Crohn''s Disease', 1)
)
    AS Source (Id, Description, Active)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[DietaryRequirements] AS Target
USING (VALUES
           ('C5614AA0-E50C-4BCF-B59F-A55A43B271C8', 'Artificial Colouring Allergy'),
           ('C5614AA0-E51C-4BCF-B59F-A55A43B271C8', 'Gluten Free'),
           ('C5614AA0-E52C-4BCF-B59F-A55A43B271C8', 'Halal'),
           ('C5614AA0-E53C-4BCF-B59F-A55A43B271C8', 'Kosher Foods Only'),
           ('C5614AA0-E54C-4BCF-B59F-A55A43B271C8', 'No Dairy Produce'),
           ('C5614AA0-E55C-4BCF-B59F-A55A43B271C8', 'No Nuts of Any Type/Quantity'),
           ('C5614AA0-E56C-4BCF-B59F-A55A43B271C8', 'No Pork'),
           ('C5614AA0-E57C-4BCF-B59F-A55A43B271C8', 'Seafood Allergy'),
           ('C5614AA0-E58C-4BCF-B59F-A55A43B271C8', 'Vegetarian')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[RelationshipTypes] AS Target
USING (VALUES
           ('4266BFD8-7983-4324-B741-AD5FAEC36688', 'Mother'),
           ('4366BFD8-7983-4324-B741-AD5FAEC36688', 'Father'),
           ('4466BFD8-7983-4324-B741-AD5FAEC36688', 'Other Family Member'),
           ('4566BFD8-7983-4324-B741-AD5FAEC36688', 'Other Relative'),
           ('4666BFD8-7983-4324-B741-AD5FAEC36688', 'Social Worker'),
           ('4766BFD8-7983-4324-B741-AD5FAEC36688', 'Religious/Spiritual Contact'),
           ('4866BFD8-7983-4324-B741-AD5FAEC36688', 'Childminder'),
           ('4966BFD8-7983-4324-B741-AD5FAEC36688', 'Foster Father'),
           ('4A66BFD8-7983-4324-B741-AD5FAEC36688', 'Foster Mother'),
           ('4B66BFD8-7983-4324-B741-AD5FAEC36688', 'Head Teacher'),
           ('4C66BFD8-7983-4324-B741-AD5FAEC36688', 'Step Father'),
           ('4D66BFD8-7983-4324-B741-AD5FAEC36688', 'Step Mother'),
           ('4E66BFD8-7983-4324-B741-AD5FAEC36688', 'Doctor'),
           ('4F66BFD8-7983-4324-B741-AD5FAEC36688', 'Carer'),
           ('5066BFD8-7983-4324-B741-AD5FAEC36688', 'Other Contact Type')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[ObservationOutcomes] AS Target
USING (VALUES
           ('B7F2DB44-A088-4B51-85DB-0429490E5212', 'Outstanding', '#0097e3'),
           ('B7F2DB45-A088-4B51-85DB-0429490E5212', 'Good', '#00e047'),
           ('B7F2DB46-A088-4B51-85DB-0429490E5212', 'Satisfactory', '#e3c500'),
           ('B7F2DB47-A088-4B51-85DB-0429490E5212', 'Requires Improvement', '#e60000')
)
    AS Source (Id, Description, ColourCode)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, ColourCode = Source.ColourCode

WHEN NOT MATCHED THEN
    INSERT (Id, Description, ColourCode, Active)
    VALUES (Id, Description, ColourCode, 1);

MERGE INTO [dbo].[TrainingCertificateStatus] AS Target
USING (VALUES
           ('28F35A25-62FC-4944-96BA-D55A956243AA', 'Completed', '#00e047'),
           ('28F35A26-62FC-4944-96BA-D55A956243AA', 'In Progress', '#e3c500'),
           ('28F35A27-62FC-4944-96BA-D55A956243AA', 'Not Started', '#878686'),
           ('28F35A28-62FC-4944-96BA-D55A956243AA', 'Failed', '#e60000')
)
    AS Source (Id, Description, ColourCode)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, ColourCode, Active)
    VALUES (Id, Description, ColourCode, 1);

MERGE INTO [dbo].[CommentBanks] AS Target
USING (VALUES
           ('24163807-8A48-4266-965F-32C5C98C6BFC', 'Attendance', 1),
           ('24263807-8A48-4266-965F-32C5C98C6BFC', 'Attitude to Learning', 1),
           ('24363807-8A48-4266-965F-32C5C98C6BFC', 'Attitude to Home Learning', 1),
           ('24463807-8A48-4266-965F-32C5C98C6BFC', 'Conduct', 1),
           ('24563807-8A48-4266-965F-32C5C98C6BFC', 'Examinations', 1),
           ('24663807-8A48-4266-965F-32C5C98C6BFC', 'Extra Curricular', 1),
           ('24763807-8A48-4266-965F-32C5C98C6BFC', 'Finance', 1),
           ('24863807-8A48-4266-965F-32C5C98C6BFC', 'General', 1),
           ('24963807-8A48-4266-965F-32C5C98C6BFC', 'Medical', 1),
           ('24A63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Behaviour', 1),
           ('24B63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Closing', 1),
           ('24C63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Communication', 1),
           ('24D63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Personal Attributes', 1),
           ('24E63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Set Goal', 1),
           ('24F63807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Social Skills', 1),
           ('25063807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Work Habits', 1),
           ('25163807-8A48-4266-965F-32C5C98C6BFC', 'Pastoral - Writing', 1),
           ('25263807-8A48-4266-965F-32C5C98C6BFC', 'Special Education Needs and Disability', 1)
)
    AS Source (Id, Description, Active)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = Source.Active

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[LogNoteTypes] AS Target
USING (VALUES
           ('C6C718BE-8255-4D26-96C1-3B92815F358E', 'Academic Support', '#4287f5', 'fa-comments'),
           ('C6C728BE-8255-4D26-96C1-3B92815F358E', 'Behaviour Log', '#f54263', 'fa-exclamation-triangle'),
           ('C6C738BE-8255-4D26-96C1-3B92815F358E', 'Medical Event', '#37bfa4', 'fa-first-aid'),
           ('C6C748BE-8255-4D26-96C1-3B92815F358E', 'Praise', '#32a852', 'fa-smile'),
           ('C6C758BE-8255-4D26-96C1-3B92815F358E', 'Report', '#000000', 'fa-clipboard'),
           ('C6C768BE-8255-4D26-96C1-3B92815F358E', 'SEN Note', '#9e0096', 'fa-hands-helping'),
           ('C6C778BE-8255-4D26-96C1-3B92815F358E', 'Student Feed', '#996900', 'fa-user-graduate'),
           ('C6C788BE-8255-4D26-96C1-3B92815F358E', 'Tutor Note', '#1b0085', 'fa-user-tie')
)
    AS Source (Id, Description, ColourCode, IconClass)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, ColourCode, IconClass, Active)
    VALUES (Id, Description, ColourCode, IconClass, 1);

MERGE INTO [dbo].[GovernanceTypes] AS Target
USING (VALUES
           ('DE23BD7A-5A3C-452C-BF58-CACFE574B631', 'Community', 'CO'),
           ('DE24BD7A-5A3C-452C-BF58-CACFE574B631', 'Voluntary Aided', 'VA'),
           ('DE25BD7A-5A3C-452C-BF58-CACFE574B631', 'Voluntary Controlled', 'VC'),
           ('DE26BD7A-5A3C-452C-BF58-CACFE574B631', 'Foundation', 'FO'),
           ('DE27BD7A-5A3C-452C-BF58-CACFE574B631', 'Independent', 'IN'),
           ('DE28BD7A-5A3C-452C-BF58-CACFE574B631', 'Non-Maintained', 'NM'),
           ('DE29BD7A-5A3C-452C-BF58-CACFE574B631', 'City Technology College', 'CT'),
           ('DE2ABD7A-5A3C-452C-BF58-CACFE574B631', 'Academies', 'CA')
)
    AS Source (Id, Description, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1);

MERGE INTO [dbo].[IntakeTypes] AS Target
USING (VALUES
           ('611BD40E-346E-4099-8450-DC8B57B81681', 'Comprehensive', 'COMP'),
           ('612BD40E-346E-4099-8450-DC8B57B81681', 'Selective (Grammar)', 'SEL1'),
           ('613BD40E-346E-4099-8450-DC8B57B81681', 'Selective (Modern Secondary)', 'SEL2'),
           ('614BD40E-346E-4099-8450-DC8B57B81681', 'Selective (Technical)', 'SEL3'),
           ('615BD40E-346E-4099-8450-DC8B57B81681', 'Selective (Religion)', 'SEL4'),
           ('616BD40E-346E-4099-8450-DC8B57B81681', 'Special', 'SPEC'),
           ('617BD40E-346E-4099-8450-DC8B57B81681', 'Hospital Special', 'HOSP')
)
    AS Source (Id, Description, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1);

MERGE INTO [dbo].[Locations] AS Target
USING (VALUES
           ('79D486A7-1C33-4A70-A883-0C005995E27C', 'Bus'),
           ('79D486A7-1C33-4A71-A883-0C005995E27C', 'Classroom'),
           ('79D486A7-1C33-4A72-A883-0C005995E27C', 'Corridor'),
           ('79D486A7-1C33-4A73-A883-0C005995E27C', 'Detention'),
           ('79D486A7-1C33-4A74-A883-0C005995E27C', 'Dinner Hall'),
           ('79D486A7-1C33-4A75-A883-0C005995E27C', 'Foyer'),
           ('79D486A7-1C33-4A76-A883-0C005995E27C', 'Gymnasium'),
           ('79D486A7-1C33-4A77-A883-0C005995E27C', 'Outside School'),
           ('79D486A7-1C33-4A78-A883-0C005995E27C', 'Playground'),
           ('79D486A7-1C33-4A79-A883-0C005995E27C', 'Playing Field'),
           ('79D486A7-1C33-4A7A-A883-0C005995E27C', 'Reception'),
           ('79D486A7-1C33-4A7B-A883-0C005995E27C', 'Sports Centre'),
           ('79D486A7-1C33-4A7C-A883-0C005995E27C', 'Staff Room'),
           ('79D486A7-1C33-4A7D-A883-0C005995E27C', 'Swimming Pool'),
           ('79D486A7-1C33-4A7E-A883-0C005995E27C', 'Toilets')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, System)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[SchoolPhases] AS Target
USING (VALUES
           ('5C75D59C-CE4B-4568-A95D-7E27F284B168', 'Primary', 'PS'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B168', 'Secondary', 'SS'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B169', 'Nursery', 'NS'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B16A', 'Middle (deemed primary)', 'MP'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B16B', 'Middle (deemed secondary)', 'MS'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B16C', 'All-through', 'AT'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B16D', 'Special', 'SP'),
           ('5C75D59C-CF4B-4568-A95D-7E27F284B16E', 'Pupil referral unit (PRU) / alternative provision (AP)', 'PR')
)
    AS Source (Id, Description, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1);

MERGE INTO [dbo].[SchoolTypes] AS Target
USING (VALUES
           ('9090AA37-A34C-4914-B85F-FE56B140B38B', 'Academies', '49'),
           ('90A0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive All-through 11-16', '21'),
           ('90B0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive All-through 11-18', '22'),
           ('90C0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive Upper School 12-15/16', '09'),
           ('90D0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive Upper School 12-18', '10'),
           ('90E0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive Upper School 13-16', '11'),
           ('90F0AA37-A34C-4914-B85F-FE56B140B38B', 'Comprehensive Upper School 13-18', '12'),
           ('9100AA37-A34C-4914-B85F-FE56B140B38B', 'Junior Comprehensive, 11-13, automatic transfer', '25'),
           ('9110AA37-A34C-4914-B85F-FE56B140B38B', 'Junior Comprehensive, 11-16, optional transfer at 14', '28'),
           ('9120AA37-A34C-4914-B85F-FE56B140B38B', 'Junior Comprehensive, 11-14, automatic transfer', '26'),
           ('9130AA37-A34C-4914-B85F-FE56B140B38B', 'Junior Comprehensive, 11-16, optional transfer at 13', '27'),
           ('9140AA37-A34C-4914-B85F-FE56B140B38B', 'Middle school (10-13), deemed secondary', '08'),
           ('9160AA37-A34C-4914-B85F-FE56B140B38B', 'Middle school (9-13), deemed secondary', '07'),
           ('9170AA37-A34C-4914-B85F-FE56B140B38B', 'Senior Comprehensive, 13-16, automatic transfer', '29'),
           ('9180AA37-A34C-4914-B85F-FE56B140B38B', 'Senior Comprehensive, 13-18, automatic transfer', '31'),
           ('9190AA37-A34C-4914-B85F-FE56B140B38B', 'Senior Comprehensive, 13-18, optional transfer', '30'),
           ('91A0AA37-A34C-4914-B85F-FE56B140B38B', 'Senior Comprehensive, 14-18, automatic transfer', '33'),
           ('91B0AA37-A34C-4914-B85F-FE56B140B38B', 'Senior Comprehensive, 14-18, optional transfer', '32')
)
    AS Source (Id, Description, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1);

MERGE INTO [dbo].[SenEventTypes] AS Target
USING (VALUES
           ('4A504CAF-290C-4BF9-8E7E-56054BE94CBC', 'Audiometrist'),
           ('4A505CAF-290C-4BF9-8E7E-56054BE94CBC', 'Behaviour Modification Therapist'),
           ('4A506CAF-290C-4BF9-8E7E-56054BE94CBC', 'Meeting (1-1)'),
           ('4A507CAF-290C-4BF9-8E7E-56054BE94CBC', 'Meeting (Group)'),
           ('4A508CAF-290C-4BF9-8E7E-56054BE94CBC', 'Medical'),
           ('4A509CAF-290C-4BF9-8E7E-56054BE94CBC', 'Physiotherapist'),
           ('4A50ACAF-290C-4BF9-8E7E-56054BE94CBC', 'Telephone Call'),
           ('4A50BCAF-290C-4BF9-8E7E-56054BE94CBC', 'External Specialist Visit'),
           ('4A50CCAF-290C-4BF9-8E7E-56054BE94CBC', 'Incident'),
           ('4A50DCAF-290C-4BF9-8E7E-56054BE94CBC', 'Parental Contact'),
           ('4A50ECAF-290C-4BF9-8E7E-56054BE94CBC', 'Social Worker'),
           ('4A50FCAF-290C-4BF9-8E7E-56054BE94CBC', 'Speech Therapist'),
           ('4A510CAF-290C-4BF9-8E7E-56054BE94CBC', 'Other')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[SenProvisionTypes] AS Target
USING (VALUES
           ('D568AE97-8137-4B56-9316-E6CA4C8327EF', 'Extra Time for Exams'),
           ('D568AE97-8147-4B56-9316-E6CA4C8327EF', 'IT Provision'),
           ('D568AE97-8157-4B56-9316-E6CA4C8327EF', 'Non Teaching Assistant (General)'),
           ('D568AE97-8167-4B56-9316-E6CA4C8327EF', 'Not Specified'),
           ('D568AE97-8177-4B56-9316-E6CA4C8327EF', 'Physiotherapy'),
           ('D568AE97-8187-4B56-9316-E6CA4C8327EF', 'Resourced Provision'),
           ('D568AE97-8197-4B56-9316-E6CA4C8327EF', 'Site Access Facilities'),
           ('D568AE97-81A7-4B56-9316-E6CA4C8327EF', 'Special Needs Support Assistant'),
           ('D568AE97-81B7-4B56-9316-E6CA4C8327EF', 'Speech Therapy'),
           ('D568AE97-81C7-4B56-9316-E6CA4C8327EF', 'Time in SEN Unit'),
           ('D568AE97-81D7-4B56-9316-E6CA4C8327EF', 'Time in Specialist Class')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[SenReviewTypes] AS Target
USING (VALUES
           ('D635E1BA-B3AB-4FD0-9B84-94A77CC69271', 'Annual'),
           ('D635E2BA-B3AB-4FD0-9B84-94A77CC69271', 'Initial'),
           ('D635E3BA-B3AB-4FD0-9B84-94A77CC69271', 'Other'),
           ('D635E4BA-B3AB-4FD0-9B84-94A77CC69271', 'Statement Annual Review'),
           ('D635E5BA-B3AB-4FD0-9B84-94A77CC69271', 'Statement End'),
           ('D635E6BA-B3AB-4FD0-9B84-94A77CC69271', 'Statement Start'),
           ('D635E7BA-B3AB-4FD0-9B84-94A77CC69271', 'Termly')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[SenStatus] AS Target
USING (VALUES
           ('71D39DF5-B0CA-4EAA-AD51-5B07C2304F27', 'N', 'No Special Educational Need'),
           ('71D39DF5-B1CA-4EAA-AD51-5B07C2304F27', 'O', 'Provision No Longer Needed'),
           ('71D39DF5-B2CA-4EAA-AD51-5B07C2304F27', 'Q', 'School Action+ and Statutory Assessment'),
           ('71D39DF5-B3CA-4EAA-AD51-5B07C2304F27', 'A', 'School/Early Years Action'),
           ('71D39DF5-B4CA-4EAA-AD51-5B07C2304F27', 'P', 'School/Early Years Action+'),
           ('71D39DF5-B5CA-4EAA-AD51-5B07C2304F27', 'K', 'SEN Support'),
           ('71D39DF5-B6CA-4EAA-AD51-5B07C2304F27', 'X', 'Severe Educational Disability'),
           ('71D39DF5-B7CA-4EAA-AD51-5B07C2304F27', 'S', 'Statement')
)
    AS Source (Id, Code, Description)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Code, Description, Active)
    VALUES (Id, Code, Description, 1);

MERGE INTO [dbo].[SenTypes] AS Target
USING (VALUES
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E506B', 'SPLD', 'Specific Learning Difficulty'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E506C', 'MLD', 'Moderate Learning Difficulty'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E506D', 'SLD', 'Severe Learning Difficulty'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E506E', 'PMLD', 'Profound and Multiple Learning Difficulty'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E506F', 'SEMH', 'Social, Emotional and Mental Health'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5070', 'SLCN', 'Speech, Language and Communication Needs'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5071', 'HI', 'Hearing Impairment'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5072', 'VI', 'Vision Impairment'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5073', 'MSI', 'Multi-Sensory Impairment'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5074', 'PD', 'Physical Disability'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5075', 'ASD', 'Autistic Spectrum Disorder'),
           ('2E7EE554-CDBA-4237-88D2-B8A1E93E5076', 'OTH', 'Other Difficulties/Disability')
)
    AS Source (Id, Code, Description)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Code, Description, Active)
    VALUES (Id, Code, Description, 1);

MERGE INTO [dbo].[DiaryEventAttendeeResponses] AS Target
USING (VALUES
           ('4F62DA79-A0A7-409A-B284-79150E7ACDD1', 'Accepted'),
           ('4F62DA89-A0A7-409A-B284-79150E7ACDD1','Tentative'),
           ('4F62DA99-A0A7-409A-B284-79150E7ACDD1','Declined')
)
    AS Source (Id, Description)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, 1);

MERGE INTO [dbo].[LocalAuthorities] AS Target
USING (VALUES
           ('30121585-D9AE-44AC-B2E7-AA99D40E4F9D','301','Barking and Dagenham'),
           ('30221585-D9AE-44AC-B2E7-AA99D40E4F9D','302','Barnet'),
           ('30321585-D9AE-44AC-B2E7-AA99D40E4F9D','370','Barnsley'),
           ('30421585-D9AE-44AC-B2E7-AA99D40E4F9D','800','Bath and North East Somerset'),
           ('30521585-D9AE-44AC-B2E7-AA99D40E4F9D','822','Bedford'),
           ('30621585-D9AE-44AC-B2E7-AA99D40E4F9D','303','Bexley'),
           ('30721585-D9AE-44AC-B2E7-AA99D40E4F9D','330','Birmingham'),
           ('30821585-D9AE-44AC-B2E7-AA99D40E4F9D','889','Blackburn with Darwen'),
           ('30921585-D9AE-44AC-B2E7-AA99D40E4F9D','890','Blackpool'),
           ('30A21585-D9AE-44AC-B2E7-AA99D40E4F9D','350','Bolton'),
           ('30B21585-D9AE-44AC-B2E7-AA99D40E4F9D','837','Bournemouth'),
           ('30C21585-D9AE-44AC-B2E7-AA99D40E4F9D','867','Bracknell Forest'),
           ('30D21585-D9AE-44AC-B2E7-AA99D40E4F9D','380','Bradford'),
           ('30E21585-D9AE-44AC-B2E7-AA99D40E4F9D','304','Brent'),
           ('30F21585-D9AE-44AC-B2E7-AA99D40E4F9D','846','Brighton and Hove'),
           ('31021585-D9AE-44AC-B2E7-AA99D40E4F9D','801','Bristol'),
           ('31121585-D9AE-44AC-B2E7-AA99D40E4F9D','825','Buckinghamshire'),
           ('31221585-D9AE-44AC-B2E7-AA99D40E4F9D','351','Bury'),
           ('31321585-D9AE-44AC-B2E7-AA99D40E4F9D','381','Calderdale'),
           ('31421585-D9AE-44AC-B2E7-AA99D40E4F9D','873','Cambridgeshire'),
           ('31521585-D9AE-44AC-B2E7-AA99D40E4F9D','202','Camden'),
           ('31621585-D9AE-44AC-B2E7-AA99D40E4F9D','823','Central Bedfordshire'),
           ('31721585-D9AE-44AC-B2E7-AA99D40E4F9D','895','Cheshire East'),
           ('31821585-D9AE-44AC-B2E7-AA99D40E4F9D','896','Cheshire West and Chester'),
           ('31921585-D9AE-44AC-B2E7-AA99D40E4F9D','201','City of London'),
           ('31A21585-D9AE-44AC-B2E7-AA99D40E4F9D','908','Cornwall'),
           ('31B21585-D9AE-44AC-B2E7-AA99D40E4F9D','331','Coventry'),
           ('31C21585-D9AE-44AC-B2E7-AA99D40E4F9D','306','Croydon'),
           ('31D21585-D9AE-44AC-B2E7-AA99D40E4F9D','909','Cumbria'),
           ('31E21585-D9AE-44AC-B2E7-AA99D40E4F9D','841','Darlington'),
           ('31F21585-D9AE-44AC-B2E7-AA99D40E4F9D','831','Derby'),
           ('32021585-D9AE-44AC-B2E7-AA99D40E4F9D','830','Derbyshire'),
           ('32121585-D9AE-44AC-B2E7-AA99D40E4F9D','878','Devon'),
           ('32221585-D9AE-44AC-B2E7-AA99D40E4F9D','371','Doncaster'),
           ('32321585-D9AE-44AC-B2E7-AA99D40E4F9D','835','Dorset'),
           ('32421585-D9AE-44AC-B2E7-AA99D40E4F9D','332','Dudley'),
           ('32521585-D9AE-44AC-B2E7-AA99D40E4F9D','840','Durham'),
           ('32621585-D9AE-44AC-B2E7-AA99D40E4F9D','307','Ealing'),
           ('32721585-D9AE-44AC-B2E7-AA99D40E4F9D','811','East Riding of Yorkshire'),
           ('32821585-D9AE-44AC-B2E7-AA99D40E4F9D','845','East Sussex'),
           ('32921585-D9AE-44AC-B2E7-AA99D40E4F9D','308','Enfield'),
           ('32A21585-D9AE-44AC-B2E7-AA99D40E4F9D','881','Essex'),
           ('32B21585-D9AE-44AC-B2E7-AA99D40E4F9D','390','Gateshead'),
           ('32C21585-D9AE-44AC-B2E7-AA99D40E4F9D','916','Gloucestershire'),
           ('32D21585-D9AE-44AC-B2E7-AA99D40E4F9D','203','Greenwich'),
           ('32E21585-D9AE-44AC-B2E7-AA99D40E4F9D','204','Hackney'),
           ('32F21585-D9AE-44AC-B2E7-AA99D40E4F9D','876','Halton'),
           ('33021585-D9AE-44AC-B2E7-AA99D40E4F9D','205','Hammersmith and Fulham'),
           ('33121585-D9AE-44AC-B2E7-AA99D40E4F9D','850','Hampshire'),
           ('33221585-D9AE-44AC-B2E7-AA99D40E4F9D','309','Haringey'),
           ('33321585-D9AE-44AC-B2E7-AA99D40E4F9D','310','Harrow'),
           ('33421585-D9AE-44AC-B2E7-AA99D40E4F9D','805','Hartlepool'),
           ('33521585-D9AE-44AC-B2E7-AA99D40E4F9D','311','Havering'),
           ('33621585-D9AE-44AC-B2E7-AA99D40E4F9D','884','Hereforshire'),
           ('33721585-D9AE-44AC-B2E7-AA99D40E4F9D','919','Hertforshire'),
           ('33821585-D9AE-44AC-B2E7-AA99D40E4F9D','312','Hillingdon'),
           ('33921585-D9AE-44AC-B2E7-AA99D40E4F9D','313','Hounslow'),
           ('33A21585-D9AE-44AC-B2E7-AA99D40E4F9D','921','Isle of Wight'),
           ('33B21585-D9AE-44AC-B2E7-AA99D40E4F9D','420','Isles of Scilly'),
           ('33C21585-D9AE-44AC-B2E7-AA99D40E4F9D','206','Islington'),
           ('33D21585-D9AE-44AC-B2E7-AA99D40E4F9D','207','Kensington and Chelsea'),
           ('33E21585-D9AE-44AC-B2E7-AA99D40E4F9D','886','Kent'),
           ('33F21585-D9AE-44AC-B2E7-AA99D40E4F9D','810','Kingston upon Hull'),
           ('34021585-D9AE-44AC-B2E7-AA99D40E4F9D','314','Kingston upon Thames'),
           ('34121585-D9AE-44AC-B2E7-AA99D40E4F9D','382','Kirklees'),
           ('34221585-D9AE-44AC-B2E7-AA99D40E4F9D','340','Knowsley'),
           ('34321585-D9AE-44AC-B2E7-AA99D40E4F9D','208','Lambeth'),
           ('34421585-D9AE-44AC-B2E7-AA99D40E4F9D','888','Lancashire'),
           ('34521585-D9AE-44AC-B2E7-AA99D40E4F9D','383','Leeds'),
           ('34621585-D9AE-44AC-B2E7-AA99D40E4F9D','856','Leicester'),
           ('34721585-D9AE-44AC-B2E7-AA99D40E4F9D','855','Leicestershire'),
           ('34821585-D9AE-44AC-B2E7-AA99D40E4F9D','209','Lewisham'),
           ('34921585-D9AE-44AC-B2E7-AA99D40E4F9D','925','Lincolnshire'),
           ('34A21585-D9AE-44AC-B2E7-AA99D40E4F9D','341','Liverpool'),
           ('34B21585-D9AE-44AC-B2E7-AA99D40E4F9D','821','Luton'),
           ('34C21585-D9AE-44AC-B2E7-AA99D40E4F9D','352','Manchester'),
           ('34D21585-D9AE-44AC-B2E7-AA99D40E4F9D','887','Medway'),
           ('34E21585-D9AE-44AC-B2E7-AA99D40E4F9D','315','Merton'),
           ('34F21585-D9AE-44AC-B2E7-AA99D40E4F9D','806','Middlesborough'),
           ('35021585-D9AE-44AC-B2E7-AA99D40E4F9D','826','Milton Keynes'),
           ('35121585-D9AE-44AC-B2E7-AA99D40E4F9D','391','Newcastle upon Tyne'),
           ('35221585-D9AE-44AC-B2E7-AA99D40E4F9D','316','Newham'),
           ('35321585-D9AE-44AC-B2E7-AA99D40E4F9D','926','Norfolk'),
           ('35421585-D9AE-44AC-B2E7-AA99D40E4F9D','812','North East Lincolnshire'),
           ('35521585-D9AE-44AC-B2E7-AA99D40E4F9D','813','North Lincolnshire'),
           ('35621585-D9AE-44AC-B2E7-AA99D40E4F9D','802','North Somerset'),
           ('35721585-D9AE-44AC-B2E7-AA99D40E4F9D','392','North Tyneside'),
           ('35821585-D9AE-44AC-B2E7-AA99D40E4F9D','815','North Yorkshire'),
           ('35921585-D9AE-44AC-B2E7-AA99D40E4F9D','928','Northamptonshire'),
           ('35A21585-D9AE-44AC-B2E7-AA99D40E4F9D','929','Northumberland'),
           ('35B21585-D9AE-44AC-B2E7-AA99D40E4F9D','892','Nottingham'),
           ('35C21585-D9AE-44AC-B2E7-AA99D40E4F9D','891','Nottinghamshire'),
           ('35D21585-D9AE-44AC-B2E7-AA99D40E4F9D','353','Oldham'),
           ('35E21585-D9AE-44AC-B2E7-AA99D40E4F9D','931','Oxfordshire'),
           ('35F21585-D9AE-44AC-B2E7-AA99D40E4F9D','874','Peterborough'),
           ('36021585-D9AE-44AC-B2E7-AA99D40E4F9D','879','Plymouth'),
           ('36121585-D9AE-44AC-B2E7-AA99D40E4F9D','836','Poole'),
           ('36221585-D9AE-44AC-B2E7-AA99D40E4F9D','851','Portsmouth'),
           ('36321585-D9AE-44AC-B2E7-AA99D40E4F9D','870','Reading'),
           ('36421585-D9AE-44AC-B2E7-AA99D40E4F9D','317','Redbridge'),
           ('36521585-D9AE-44AC-B2E7-AA99D40E4F9D','807','Redcar and Cleveland'),
           ('36621585-D9AE-44AC-B2E7-AA99D40E4F9D','318','Richmon upon Thames'),
           ('36721585-D9AE-44AC-B2E7-AA99D40E4F9D','354','Rochdale'),
           ('36821585-D9AE-44AC-B2E7-AA99D40E4F9D','372','Rotherham'),
           ('36921585-D9AE-44AC-B2E7-AA99D40E4F9D','857','Rutland'),
           ('36A21585-D9AE-44AC-B2E7-AA99D40E4F9D','355','Salford'),
           ('36B21585-D9AE-44AC-B2E7-AA99D40E4F9D','333','Sandwell'),
           ('36C21585-D9AE-44AC-B2E7-AA99D40E4F9D','343','Sefton'),
           ('36D21585-D9AE-44AC-B2E7-AA99D40E4F9D','373','Sheffield'),
           ('36E21585-D9AE-44AC-B2E7-AA99D40E4F9D','893','Shropshire'),
           ('36F21585-D9AE-44AC-B2E7-AA99D40E4F9D','871','Slough'),
           ('37021585-D9AE-44AC-B2E7-AA99D40E4F9D','334','Solihull'),
           ('37121585-D9AE-44AC-B2E7-AA99D40E4F9D','933','Somerset'),
           ('37221585-D9AE-44AC-B2E7-AA99D40E4F9D','803','South Gloucestershire'),
           ('37321585-D9AE-44AC-B2E7-AA99D40E4F9D','393','South Tyneside'),
           ('37421585-D9AE-44AC-B2E7-AA99D40E4F9D','852','Southampton'),
           ('37521585-D9AE-44AC-B2E7-AA99D40E4F9D','882','Southend on Sea'),
           ('37621585-D9AE-44AC-B2E7-AA99D40E4F9D','210','Southwark'),
           ('37721585-D9AE-44AC-B2E7-AA99D40E4F9D','342','St Helens'),
           ('37821585-D9AE-44AC-B2E7-AA99D40E4F9D','860','Staffordshire'),
           ('37921585-D9AE-44AC-B2E7-AA99D40E4F9D','356','Stockport'),
           ('37A21585-D9AE-44AC-B2E7-AA99D40E4F9D','808','Stockton-on-Tees'),
           ('37B21585-D9AE-44AC-B2E7-AA99D40E4F9D','861','Stoke-on-Trent'),
           ('37C21585-D9AE-44AC-B2E7-AA99D40E4F9D','935','Suffolk'),
           ('37D21585-D9AE-44AC-B2E7-AA99D40E4F9D','394','Sunderland'),
           ('37E21585-D9AE-44AC-B2E7-AA99D40E4F9D','936','Surrey'),
           ('37F21585-D9AE-44AC-B2E7-AA99D40E4F9D','319','Sutton'),
           ('38021585-D9AE-44AC-B2E7-AA99D40E4F9D','866','Swindon'),
           ('38121585-D9AE-44AC-B2E7-AA99D40E4F9D','357','Tameside'),
           ('38221585-D9AE-44AC-B2E7-AA99D40E4F9D','894','Telford and Wrekin'),
           ('38321585-D9AE-44AC-B2E7-AA99D40E4F9D','883','Thurrock'),
           ('38521585-D9AE-44AC-B2E7-AA99D40E4F9D','880','Torbay'),
           ('38621585-D9AE-44AC-B2E7-AA99D40E4F9D','211','Tower Hamlets'),
           ('38721585-D9AE-44AC-B2E7-AA99D40E4F9D','358','Trafford'),
           ('38821585-D9AE-44AC-B2E7-AA99D40E4F9D','384','Wakefield'),
           ('38921585-D9AE-44AC-B2E7-AA99D40E4F9D','335','Walsall'),
           ('38A21585-D9AE-44AC-B2E7-AA99D40E4F9D','320','Waltham Forest'),
           ('38B21585-D9AE-44AC-B2E7-AA99D40E4F9D','212','Wandsworth'),
           ('38C21585-D9AE-44AC-B2E7-AA99D40E4F9D','877','Warrington'),
           ('38D21585-D9AE-44AC-B2E7-AA99D40E4F9D','937','Warwickshire'),
           ('38E21585-D9AE-44AC-B2E7-AA99D40E4F9D','869','West Berkshire'),
           ('38F21585-D9AE-44AC-B2E7-AA99D40E4F9D','938','West Sussex'),
           ('39021585-D9AE-44AC-B2E7-AA99D40E4F9D','213','Westminster'),
           ('39121585-D9AE-44AC-B2E7-AA99D40E4F9D','865','Wiltshire'),
           ('39221585-D9AE-44AC-B2E7-AA99D40E4F9D','868','Windsor and Maidenhead'),
           ('39321585-D9AE-44AC-B2E7-AA99D40E4F9D','344','Wirral'),
           ('39421585-D9AE-44AC-B2E7-AA99D40E4F9D','872','Wokingham'),
           ('39521585-D9AE-44AC-B2E7-AA99D40E4F9D','336','Wolverhampton'),
           ('39621585-D9AE-44AC-B2E7-AA99D40E4F9D','885','Worcestershire'),
           ('39721585-D9AE-44AC-B2E7-AA99D40E4F9D','816','York')
)
    AS Source (Id, LeaCode, Name)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET LeaCode = Source.LeaCode, Name = Source.Name

WHEN NOT MATCHED THEN
    INSERT(Id, LeaCode, Name)
    VALUES (Id, LeaCode, Name);

MERGE INTO [dbo].[CurriculumYearGroups] AS Target
USING (VALUES
           ('0D970DEF-13CE-483E-AD8A-6608D444F306','Early First Year', '0', 'E1'),
           ('0D971DEF-13CE-483E-AD8A-6608D444F306','Early Second Year', '0', 'E2'),
           ('0D972DEF-13CE-483E-AD8A-6608D444F306','Nursery First Year', '0', 'N1'),
           ('0D973DEF-13CE-483E-AD8A-6608D444F306','Nursery Second Year', '0', 'N2'),
           ('0D974DEF-13CE-483E-AD8A-6608D444F306','Reception', '0', 'R'),
           ('0D975DEF-13CE-483E-AD8A-6608D444F306','Year 1', '1', '1'),
           ('0D976DEF-13CE-483E-AD8A-6608D444F306','Year 2', '1', '2'),
           ('0D977DEF-13CE-483E-AD8A-6608D444F306','Year 3', '2', '3'),
           ('0D978DEF-13CE-483E-AD8A-6608D444F306','Year 4', '2', '4'),
           ('0D979DEF-13CE-483E-AD8A-6608D444F306','Year 5', '2', '5'),
           ('0D97ADEF-13CE-483E-AD8A-6608D444F306','Year 6', '2', '6'),
           ('0D97BDEF-13CE-483E-AD8A-6608D444F306','Year 7', '3', '7'),
           ('0D97CDEF-13CE-483E-AD8A-6608D444F306','Year 8', '3', '8'),
           ('0D97DDEF-13CE-483E-AD8A-6608D444F306','Year 9', '3', '9'),
           ('0D97EDEF-13CE-483E-AD8A-6608D444F306','Year 10', '4', '10'),
           ('0D97FDEF-13CE-483E-AD8A-6608D444F306','Year 11', '4', '11'),
           ('0D980DEF-13CE-483E-AD8A-6608D444F306','Year 12', '5', '12'),
           ('0D981DEF-13CE-483E-AD8A-6608D444F306','Year 13', '5', '13'),
           ('0D982DEF-13CE-483E-AD8A-6608D444F306','Year 14', '5', '14')
)
    AS Source (Id, Name, KeyStage, Code)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET Name = Source.Name, KeyStage = Source.KeyStage

WHEN NOT MATCHED THEN
    INSERT (Id, Name, KeyStage, Code)
    VALUES (Id, Name, KeyStage, Code);

MERGE INTO [dbo].[Directories] AS Target
USING (VALUES
           ('B5DBF3AE-D9A9-4502-AE16-E437BED14F38', NULL, 'root', 0, 0)
)
    AS Source (Id, ParentId, Name, Restricted, Private)
ON Target.Id = Source.Id

WHEN MATCHED THEN
    UPDATE SET ParentId = Source.ParentId, Name = Source.Name, Private = Source.Private

WHEN NOT MATCHED THEN
    INSERT (Id, ParentId, Name, Private)
    VALUES (Id, ParentId, Name, Private);

MERGE INTO [dbo].[DiaryEventTypes] AS Target
USING (VALUES
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F22','EC Activity', '1', '#c2fdff', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F23','Lesson', '1', '#2677d4', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F24','Cover', '1', '#f0f029', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F25','Detention', '1', '#d62a24', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F26','NCC', '1', '#24d6ac', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F27','PPA', '1', '#24d653', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F28','School Holiday', '1', '#c300ff', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F29','Teacher Training', '1', '#ff9500', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2B', 'Parent Evening', '1', '#d10486', 1),
           ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2A','General', '1', '#c9c9c9', 0)
)
    AS Source (Id, Description, Active, ColourCode, System)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, ColourCode, System)
    VALUES (Id, Description, Active, ColourCode, System);

MERGE INTO [dbo].[TaskTypes] AS Target
USING (VALUES
           ('606D723E-95A7-4356-AA62-1ADADBA4A1C0', 'Personal', 1, 1, '#ca03fc', 0),
           ('606D723E-95A7-4356-AA62-1ADADBA4A1C1', 'Other', 1, 0, '#6f7070', 0),
           ('606D723E-95A7-4356-AA62-1ADADBA4A1C2', 'Homework', 1, 0, '#ff4800', 1)
)
    AS Source (Id, Description, Active, Personal, ColourCode, System)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, Personal, ColourCode, System)
    VALUES (Id, Description, Active, Personal, ColourCode, System)

WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, Active = Source.Active, Personal = Source.Personal, ColourCode = Source.ColourCode, System = Source.System;

MERGE INTO [dbo].[Ethnicities] AS Target
USING (VALUES
           ('A70481B2-8AD3-4927-863A-E03882CF0307', 'White, British', 'WBRI'),
           ('A70481B2-8AD3-4927-863A-E03882CF0308', 'White, Irish', 'WIRI'),
           ('A70481B2-8AD3-4927-863A-E03882CF0309', 'White, Any other White background', 'WOTH'),
           ('A70481B2-8AD3-4927-863A-E03882CF030A', 'Asian/Asian-British, Bangladeshi', 'ABAN'),
           ('A70481B2-8AD3-4927-863A-E03882CF030B', 'Asian/Asian-British, Indian', 'AIND'),
           ('A70481B2-8AD3-4927-863A-E03882CF030C', 'Asian/Asian-British, Any other Asian background', 'AOTH'),
           ('A70481B2-8AD3-4927-863A-E03882CF030D', 'Asian/Asian-British, Pakistani', 'APKN'),
           ('A70481B2-8AD3-4927-863A-E03882CF030E', 'Black/Black-British, African', 'BAFR'),
           ('A70481B2-8AD3-4927-863A-E03882CF030F', 'Black/Black-British, Caribbean', 'BCRB'),
           ('A70481B2-8AD3-4927-863A-E03882CF0310', 'Black/Black-British, Any other Black background', 'BOTH'),
           ('A70481B2-8AD3-4927-863A-E03882CF0311', 'Chinese', 'CHNE'),
           ('A70481B2-8AD3-4927-863A-E03882CF0312', 'Mixed, Any other Mixed background', 'MOTH'),
           ('A70481B2-8AD3-4927-863A-E03882CF0313', 'Mixed White and Asian', 'MWAS'),
           ('A70481B2-8AD3-4927-863A-E03882CF0314', 'Mixed White and Black African', 'MWBA'),
           ('A70481B2-8AD3-4927-863A-E03882CF0315', 'Mixed White and Black Caribbean', 'MWBC'),
           ('A70481B2-8AD3-4927-863A-E03882CF0316', 'Any other ethnic background', 'OOTH'),
           ('A70481B2-8AD3-4927-863A-E03882CF0317', 'Refused', 'REFU'),
           ('A70481B2-8AD3-4927-863A-E03882CF0318', 'Not Obtained', 'NOBT'),
           ('A70481B2-8AD3-4927-863A-E03882CF0319', 'Traveller of Irish Heritage', 'WIRT'),
           ('A70481B2-8AD3-4927-863A-E03882CF031A', 'Gypsy/Roma', 'WROM')
) AS Source (Id, Description, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Code, Active)
    VALUES (Id, Description, Code, 1);

MERGE INTO [dbo].[VatRates] AS Target
USING (VALUES
           ('DBFB7DE3-216E-421F-97FB-B0A802AE9CCB', 'Standard Rate', 1, 20),
           ('DBFB7DE3-216E-421F-97FB-B0A802AE9CCC', 'Reduced Rate', 1, 5),
           ('DBFB7DE3-216E-421F-97FB-B0A802AE9CCD', 'Zero Rate', 1, 0)
) AS Source (Id, Description, Active, Value)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, Value)
    VALUES (Id, Description, Active, Value);

MERGE INTO [dbo].[SubjectCodeSets] AS Target
USING (VALUES
           ('3A5F93CA-E082-4EAA-96D7-C538F723502B','DfE',1),
           ('3A5F93CA-E082-4EAA-96D7-C538F723502C','QCA',1)
) AS Source (Id, Description, Active)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[SubjectCodes] AS Target
USING (VALUES
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580950', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Accountancy', 1, 'ACC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580951', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Any new GCSE in a vocational subject', 1, 'VNW'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580952', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Applied Art & Design', 1, 'AAD'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580953', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Applied Business Studies', 1, 'ABS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580954', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Applied ICT', 1, 'AIT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580955', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Applied Science', 1, 'ASC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580956', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Arabic', 1, 'ARA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580957', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design / Art', 1, 'ART'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580958', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bengali', 1, 'BEN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580959', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology / Botany / Zoology / Ecology', 1, 'BIO'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095A', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Careers Education', 1, 'CAR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095B', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chemistry', 1, 'CHM'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095C', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Child Development', 1, 'CHD'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095D', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chinese', 1, 'CHI'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095E', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Citizenship', 1, 'CIT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758095F', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Classics', 1, 'CLS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580960', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Combined Arts / Humanities / Social studies', 1, 'AHS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580961', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Combined/General Science - Biology', 1, 'CSB'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580962', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Combined/General Science - Chemistry', 1, 'CSC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580963', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Combined/General Science - Physics', 1, 'CSP'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580964', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Commercial & Business Studies/Education/Management', 1, 'CAB'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580965', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Communication Studies', 1, 'COM'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580966', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Community Studies', 1, 'COS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580967', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Computer Science', 1, 'CSI'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580968', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Construction and Built Environment / Building', 1, 'CBE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580969', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Craft, Design & Technology', 1, 'CDT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096A', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Cymraeg/Welsh (as First Language)', 1, 'CYM'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096B', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dance', 1, 'DNC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096C', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Danish', 1, 'DAN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096D', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology', 1, 'DAT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096E', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Electronics', 1, 'DTE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758096F', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Food Technology', 1, 'DTF'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580970', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Graphics', 1, 'DTG'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580971', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Resistant Materials', 1, 'DTR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580972', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Systems & Control', 1, 'DTS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580973', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology - Textiles', 1, 'DTT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580974', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Drama', 1, 'DRA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580975', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dutch', 1, 'DUT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580976', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Economics', 1, 'ECO'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580977', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Education', 1, 'EDU'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580978', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Engineering', 1, 'ENR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580979', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English', 1, 'ENG'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097A', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Environmental Science/Studies', 1, 'ENV'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097B', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'European Studies', 1, 'EUR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097C', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Finnish', 1, 'FIN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097D', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French', 1, 'FRE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097E', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'General Studies', 1, 'GEN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758097F', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geography', 1, 'GEO'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580980', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geology', 1, 'GLG'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580981', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German', 1, 'GER'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580982', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Government & Politics', 1, 'GPL'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580983', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek (Classical)', 1, 'GRC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580984', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek (Modern)', 1, 'GRE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580985', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Gujerati', 1, 'GUJ'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580986', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Health and Social Care', 1, 'HSC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580987', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hebrew (Biblical)', 1, 'HBB'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580988', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hebrew (Modern)', 1, 'HEB'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580989', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hindi', 1, 'HIN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098A', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'History', 1, 'HIS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098B', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hospitality and Catering', 1, 'HAC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098C', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Humanities', 1, 'HUM'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098D', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Industrial Studies', 1, 'IND'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098E', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Information & Communication Technology', 1, 'ICT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758098F', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian', 1, 'ITA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580990', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese', 1, 'JAP'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580991', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Key Skills (Only if <KeyStage> = 4', 1, 'KSK'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580992', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Land & Environment / Agriculture', 1, 'LAE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580993', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Latin', 1, 'LAT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580994', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Law', 1, 'LAW'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580995', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Leisure, Travel and Tourism', 1, 'LTT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580996', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Life Skills', 1, 'LIF'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580997', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Manufacturing', 1, 'MNF'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580998', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics', 1, 'MAT'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA7580999', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Media Studies', 1, 'MED'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099A', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Music', 1, 'MUS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099B', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Not Applicable', 1, 'NAP'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099C', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other (not otherwise specified)', 1, 'OTH'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099D', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Aesthetic / Practical Subject', 1, 'OPR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099E', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Business / Commercial Subject', 1, 'OBC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA758099F', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Humanities', 1, 'OHU'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Language Subject', 1, 'OLA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Mathematical Subject', 1, 'OMA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Physical Subject', 1, 'OPH'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Sciences', 1, 'OSC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Social Studies', 1, 'OSS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Technological Subject', 1, 'OTE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Vocational Subject', 1, 'OVO'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Panjabi', 1, 'PAN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Performing Arts', 1, 'PER'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809A9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Personal Social & Health Education (PSHE)', 1, 'PSH'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AA', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Philosophy', 1, 'PHL'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AB', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Physical Education / Sports', 1, 'PED'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AC', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Physics', 1, 'PHY'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AD', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Portuguese', 1, 'POR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AE', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Primary Curriculum', 1, 'PRI'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809AF', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Psychology', 1, 'PSY'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Religious Education', 1, 'REL'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Retail, Warehousing & Distribution', 1, 'RWD'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Russian', 1, 'RUS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science', 1, 'SCI'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Social Studies/Science', 1, 'SSS'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Sociology', 1, 'SOC'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish', 1, 'SPA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Special Educational Needs', 1, 'SEN'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Statistics', 1, 'STA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809B9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Swedish', 1, 'SWE'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809BA', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technical Drawing/Graphics', 1, 'TDG'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809BB', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Turkish', 1, 'TUR'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809BC', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Under-5 Activities', 1, 'UFA'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809BD', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Urdu', 1, 'URD'),
           ('0D17BB9E-BFC4-4B66-B35E-ADCEA75809BE', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh', 1, 'WEL'),
           ('be339d04-e011-417c-a737-463f2bbc83c9', '3A5F93CA-E082-4EAA-96D7-C538F723502C', 'Accounting', 1, 'AK6'),
           ('53b082e0-9be5-46ea-b6cf-304e8e29aefc', '3A5F93CA-E082-4EAA-96D7-C538F723502C', 'Accounting/Finance', 1, '7410'),
           ('c368534f-d462-4189-8a1b-594ed8e1a25d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Additional Applied Science', 1, '28'),
           ('23eb45ec-35a6-4b8d-99e0-ab7d61622809', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Additional Maths', 1, 'RB1B'),
           ('0ff35530-f20a-4d17-8909-35087a7fd26d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Additional Maths (FSMQ)', 1, 'RB1C'),
           ('bf0a27f2-59c6-41cc-994e-ba09cf0ebece', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Additional Science', 1, 'RA1C'),
           ('3c3733e3-4adb-4fad-81e5-865e8e75e90e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Adult Literacy', 1, '22'),
           ('37362d14-78c1-494f-a78b-aab9fe2414bd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Adult Numeracy', 1, '23'),
           ('82c11f41-ea68-4cee-b266-d72237aeefe9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Algebra (General)', 1, 'RB31'),
           ('cd883e66-b40e-4816-92fd-1b1448d71db6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'American Studies', 1, '4110'),
           ('691a335f-d7bc-4e53-a9e1-25a80ff826c7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Ancient History', 1, '6510'),
           ('2ebb1d4f-d8e8-493f-a716-7dcb1bc8b8fa', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Ancient History', 1, 'DB21'),
           ('53a00f3f-000c-4427-ab40-40393a02e465', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Anthropology', 1, 'EE23'),
           ('92d8b5f1-b277-401c-a473-f020e8e4f444', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Applications of Maths', 1, 'RB7J'),
           ('feb92c6d-7c9f-4429-b0e0-0c810ffe3fca', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Arabic', 1, '5910'),
           ('8295017f-9dd1-41ea-9821-9765b89054f6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Arabic', 1, 'FKM'),
           ('626681df-ae51-4927-9f84-765d48b372c3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Archaeology', 1, '4730'),
           ('c45ee407-c9bf-4517-addc-cf793bd46e40', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Archaeology', 1, 'DC'),
           ('3d52865b-b999-4191-b7c2-4a30274958d5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art', 1, '3810'),
           ('671fd3e2-2f63-462b-a968-19bee7d795de', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design', 1, '3510'),
           ('a4faf378-cc3e-4be4-b5e0-d6b7e10608a7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design  Graphics', 1, '3550'),
           ('634ea6ed-676c-436b-b107-fb4ff874f7dc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design (Voc)', 1, '1'),
           ('a2f96a14-4fb3-4d02-913b-0b68f9f06560', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design Critical Studies', 1, '3680'),
           ('c3106d66-b50a-4f56-bc00-9dd7494d532f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art & Design Drawing/Painting', 1, '3530'),
           ('866e7836-213b-4b10-a1d0-d9bb54b1efce', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art and Design  3-D Studies', 1, '3670'),
           ('ee8e8ca8-7015-4c1f-800c-be27e8a434d5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art and Design  Photography', 1, '3570'),
           ('d9d4c0d7-8c47-4599-85e5-48b2994ae704', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art and Design  Pottery', 1, '3610'),
           ('1041951d-b76b-46b5-9af2-7c1be8db62df', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art and Design  Printmaking', 1, '3630'),
           ('148bdf90-3b5c-469a-9b26-156948a45f9f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art and Design  Textiles', 1, '3650'),
           ('c76f9e0f-ce1b-4b71-9c9b-53b8b25f6c23', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art History & Criticism', 1, 'JA33'),
           ('52099d3f-0137-4bf5-9674-8f00dbf9f79d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Art with Art History', 1, '3820'),
           ('35bc9ba2-4f24-4146-81a6-5bd194a3167e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Asian Studies', 1, '4130'),
           ('9f63181b-1d1b-4e82-a554-e400edbfa2db', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Astronomy', 1, 'RE1'),
           ('8bae9e0f-0569-4d55-974b-576e61f2b39f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bengali', 1, '5930'),
           ('f0d096e4-4426-4840-8f87-082e09c501d5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bengali', 1, 'FKB'),
           ('c14c6214-b5d0-4066-a5a0-aea3cf53a53a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology', 1, '1010'),
           ('e8f6ee78-016e-44f7-94ad-2099ebaf85fd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology', 1, 'RH3'),
           ('be4cd146-98a2-4368-9d3a-21ee181f1e68', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology Human', 1, '1030'),
           ('dc71fbf1-e70b-49f8-a5be-c01b286b31dd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology Social', 1, '1050'),
           ('968b6c2e-06cb-4774-844a-13965ab90641', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Biology: Human & Social', 1, '1060'),
           ('d6f78201-cab6-4458-80f2-9cb2f74359a9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Buiness/Industry Economics', 1, 'EB62'),
           ('55a347dc-df03-4fe8-8d1e-2f7a81a086f3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bus.C.S-English Writing', 1, 'AFEW'),
           ('aa54a47a-c4a0-4481-a2c0-af40d7906ef0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bus.C.S-French Writing ', 1, 'AFFW'),
           ('34c9907b-a041-420d-93b1-55546bf83a06', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bus.C.S-German Writing', 1, 'AFGW'),
           ('7f5aa34c-d847-4b9a-a395-7dfabca16f8a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Bus.C.S-Spanish Writing', 1, 'AFSW'),
           ('96ca4db3-271a-4e6b-9336-3d829813c452', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Business & Information Studs 1', 1, '3250'),
           ('5dc758ce-a41a-42db-a926-9a3ec7e72303', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Business & Information Studs 2', 1, '3270'),
           ('30b85bb5-a6c7-4f0f-9731-1f5e69d2e4d9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Business (Voc)', 1, '2'),
           ('ca09d2d6-1b9b-4e2f-ae84-fd0b0035a1f1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Business Studies', 1, '3210'),
           ('966b45cc-d925-47d8-bbf6-fa8d6df89652', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Business Studies & Economics', 1, '3230'),
           ('a1fda7f9-915e-4f0f-809b-29fd13fe51e2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Calculus', 1, 'RB55'),
           ('dcb22652-fa29-4a98-b117-0f73c6cfd350', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Catering Studies', 1, '7430'),
           ('ee93a3ca-5ced-4f6f-b613-d50616c1fe2f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Building Studies', 1, '2910'),
           ('20476e35-a934-4a27-a2cd-ae153fe50063', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Design', 1, '2830'),
           ('df093348-93b6-43a5-903c-0e7b7e63e92c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Design/Communication', 1, '2850'),
           ('60ece04f-f459-416d-8bf7-cd4dc556f288', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Design/Realisation', 1, '2870'),
           ('77648883-3f99-48fc-a432-4e2aaff24f8a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Other', 1, '2930'),
           ('1b5fc80f-b1fa-4dd0-9db6-5d61d53331ab', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'CDT Technology', 1, '2810'),
           ('f97f63ff-3753-4de2-80a3-9f789c49f44a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chemistry', 1, '1110'),
           ('81e1e445-5f27-4580-9fd2-53581a79e2ee', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chemistry (General)', 1, 'RD1'),
           ('770a61f8-45c7-4324-b30a-2b2d0cea9d6f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chinese', 1, '5950'),
           ('c84c3204-d45a-435b-aebe-dd3d25824312', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Chinese', 1, 'FKC'),
           ('4f87ad41-ae47-4fe0-b16f-b84d0ca5dbb6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Citizenship', 1, '4910'),
           ('a383ec9e-f873-4c30-80d1-b4412b957f64', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Classical Civilisation', 1, '6530'),
           ('4e151d52-991c-473f-8c76-87bbaf7f4763', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Classical Civilisation', 1, 'DB2B'),
           ('34fa36f4-8421-4021-9b80-918adbcca12f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Classics (General)', 1, 'DB2A'),
           ('3581e591-745b-4907-8d0b-6361c4999636', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Commerce/Office Studies', 1, '7450'),
           ('fb58ce34-c7fd-46c4-ae12-d4f13e2663a4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Communication', 1, 'HD2'),
           ('d366f3eb-adc0-4228-94fb-88c7c717adf5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Communication Studies', 1, '5310'),
           ('ca0be0e8-cfaa-438b-8862-bf423824a696', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Communication Studies', 1, 'KA1'),
           ('b2d405d3-7a4d-459d-a81f-cec868d46868', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Communication Technology', 1, '2630'),
           ('c04642e9-bc71-417d-8eb4-ece189f5f5f3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Community Development', 1, 'EE31'),
           ('3d7cf37d-06ad-4b4a-98ed-a0e5ac7f2ae8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Community Studies', 1, '4750'),
           ('1ea4b05a-497b-4a64-a580-031e4f175547', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Computer Science', 1, 'CK1'),
           ('4ff6eba1-069e-4537-9409-d1ef7c546613', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Computer Studies/Computing', 1, '2610'),
           ('7f5c7404-552c-4ddd-9c11-f3deb558f123', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Computer Technology', 1, 'CJ'),
           ('8f5801ab-ecf1-44ac-8d02-ebfb74971f6a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Computer Use', 1, 'CN1'),
           ('138a18ab-2e28-4827-a466-2e29fef8613c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Construction', 1, '6'),
           ('003e757d-84a5-41ac-aa60-af1b3065c43d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Construction', 1, 'TA2'),
           ('f53cbbb0-ff7d-4808-b4d4-bc065c952f6f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Control Technology', 1, '2950'),
           ('68baeacb-d83e-40b3-a37a-c101e370a856', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Craft', 1, '3700'),
           ('8d71b515-fb7c-4418-97a2-99afd80e9a6b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Craft Design & Technology', 1, 'XA31'),
           ('12026d3d-b436-4d83-9843-4e3dbd3890a5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Creative Arts', 1, '6810'),
           ('396c6fa7-0b6c-4500-9db8-a823b3a05cf5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Creative Writing', 1, '5230'),
           ('0e4a5374-82d9-443c-8267-05252e6e2a2a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Critical Thinking', 1, '7830'),
           ('f49330bc-ba2c-4788-bcaa-8074aba622ba', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Current Affairs', 1, 'EA3'),
           ('07d6d916-6354-48d4-aba7-32dee572e703', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D & T Engineering', 1, 'VF3'),
           ('18b47028-f0f7-4684-91f1-7b7e039aa3ae', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D & T Product Design', 1, 'VF2'),
           ('b32fc726-7acc-4447-8181-42752be3a03d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D & T Resistant Materials', 1, 'XA5'),
           ('751f47c5-d595-4db7-ba8e-98af7b916bf1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D & T Textiles Technology', 1, 'XA5A'),
           ('1251016a-5fc6-454c-adef-20510014ef80', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Communication', 1, '8910'),
           ('026d13c4-582f-4c58-b4ee-161085b93d10', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Design', 1, '8920'),
           ('1dfd4be8-ae71-4237-80ca-a9fbbc8924e9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Electronic Products', 1, '9010'),
           ('e22f691e-a472-4793-8780-223ccf6df67d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Engineering', 1, '9070'),
           ('00140d51-7542-4664-b483-f152f8ed3368', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Food Technology', 1, '9020'),
           ('131f6809-9340-4245-a21c-52bcd78296e2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Graphic Products', 1, '9030'),
           ('e00827b5-95c7-4d92-90ef-50e6dbdc9840', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Product Design', 1, '9080'),
           ('a058d610-0424-41a5-b919-b979060f49b8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Resistant Materials', 1, '9040'),
           ('595c4bad-026b-4fa3-8ed8-b984ba9f633a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Systems & Control', 1, '9060'),
           ('2cb007cf-5bae-4925-919d-e0b6d44d0efd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Technology', 1, '8930'),
           ('3183fc9f-9edf-43ad-b120-23a9326cf4ce', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'D&T Textiles Technology', 1, '9050'),
           ('5881e502-8264-4cfb-a6c2-c6d78ab23d82', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dance', 1, '7230'),
           ('0220a7c7-9a29-45bb-bd7e-2dfd3149b988', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Danish', 1, '5610'),
           ('7d4208c6-8dad-497a-b65e-d40ccf54183b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design', 1, 'TA1'),
           ('e31b1505-4bbc-4406-8a02-efe67fee5f44', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design & Technology', 1, '8000'),
           ('53e2d215-8878-4728-829f-34e2c6907949', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design (Not Art & Design)', 1, '2940'),
           ('c0c5b942-d356-4428-ac07-9dabdca26591', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology', 1, '2890'),
           ('01f938cb-a900-4d16-bd43-3e464767aee2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design and Technology', 1, '8900'),
           ('fb57bd05-3c49-47f1-bdb6-bd8b5fd32adb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Design Technology/Child Devt.', 1, '8180'),
           ('acdaac06-358b-4c04-9142-67d35765bb2d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Drama', 1, '5210'),
           ('61a84e9d-a738-4a7a-9e5a-68de15ba9bc3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Drama: Community Theatre', 1, '5220'),
           ('2ef4666d-1ad0-47e7-a623-6117e964a7d1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dress', 1, '6830'),
           ('68c4c769-3f08-443b-8d7e-400c1292c451', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Art', 1, '8010'),
           ('9072c726-b62f-47a4-ae64-6118811ff3bb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Automation', 1, '8020'),
           ('f61953dd-3e52-4ac6-b75b-5781fdfbcc97', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Automotive Engineering', 1, '8030'),
           ('86ea3ffd-6485-4c6f-b69a-cd65cf664fc0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Built Environment', 1, '8040'),
           ('247ede1e-4d66-4f4c-a247-6b2f8863c3a2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Business Studies', 1, '8050'),
           ('9736460a-125e-4495-a59d-773d2744a65d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Catering', 1, '8060'),
           ('2f6dc532-2d9c-495d-847d-c2b8434c9750', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Construction', 1, '8070'),
           ('91725ca6-bed6-4b83-9af4-cac7b60ae384', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Drama', 1, '8080'),
           ('ce2a2382-3fb0-4806-b835-5da97030f603', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Economics', 1, '8090'),
           ('0b3058c3-e3eb-44ed-8f2f-addc9211be94', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Electronics', 1, '8100'),
           ('c8af259a-803d-4e02-8ba4-5ba1552eb919', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Fashion', 1, '8110'),
           ('69f9cf14-8f17-417f-900f-afa8fb3237f0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Food Industries', 1, '8120'),
           ('4b37605b-7788-44d3-a124-846d1e184c31', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Health', 1, '8130'),
           ('cdeb16b1-56c2-4d08-a4fe-8b200bc4d898', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Industry', 1, '8140'),
           ('289c6109-9472-424b-9f01-cdfb6e20adea', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Music', 1, '8150'),
           ('2efc54f8-0d81-4e6a-bfdd-49f39b688840', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Product Design', 1, '8160'),
           ('31a709eb-7917-4f8a-a709-6027920e6906', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT/Transport', 1, '8170'),
           ('f265192d-d301-49b3-a186-6464cb5e0dca', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:Control/Systems Technology', 1, '8350'),
           ('5467f631-104d-4968-a048-abbd9eac93a0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:Electronic Products & BusSt', 1, '8360'),
           ('32faf074-5c1d-43e9-8681-23ca0698c414', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:Food Technology & Business', 1, '8370'),
           ('7228bf20-cd34-4110-b6f8-9c8b72a6386a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:Graphic Products & Business', 1, '8380'),
           ('171fb93a-6b2d-4b33-92c2-113cdf431c8e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:IT', 1, '8190'),
           ('6ed3c57a-ab34-451e-9ae9-f83574041617', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'DT:Resistant Materials/Business', 1, '8390'),
           ('2a88e5a8-a177-4cae-87df-043486a523b3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dutch', 1, '5630'),
           ('6e515fea-af04-48b2-a046-e1c5c867546c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dutch', 1, 'FKD'),
           ('cf68af23-cea4-4a84-8269-857c8a975081', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Dynamic systems (Maths)', 1, 'RB56'),
           ('39e0a97b-007d-4f8b-8e30-e7fc79c3ebd4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Economics', 1, '4410'),
           ('36761f9c-f70b-466a-9236-951a8eb448d6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Economics', 1, 'EB'),
           ('f3ecee15-d1a4-45b1-920c-9ca46fb3cfa6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Economics & Business', 1, '4430'),
           ('b8899537-b109-4da1-b6f4-dfb67e7e506d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Electronic Engineering (General)', 1, 'XL1'),
           ('ab8bc950-1077-4c01-98ca-f54203240d83', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Electronics (Physics)', 1, 'RC52'),
           ('aac5b60c-07eb-4d6b-8fa0-e980e69731f0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Engineering', 1, '9'),
           ('cdae36d6-2224-4a7b-b679-d7f1cb3828fc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Engineering Studies', 1, '2970'),
           ('14275690-c2ee-442d-a751-adbc2fa9e24a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English', 1, 'FK2A'),
           ('b0376041-ecd3-479d-b521-7fc63b827ea5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English (EFL)', 1, 'FK22'),
           ('b2824ab5-7b22-4de4-92ce-9bd7bf719510', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English (KS2)', 1, '9981'),
           ('78e5cd88-4e82-4bdf-b90f-3a6a1598eace', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English (KS3)', 1, '9991'),
           ('54bdcb24-a90f-4079-a1eb-4f06dba88c6d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English in Dual Award', 1, '5150'),
           ('d6d00030-ed74-4acf-8edf-072a4ac48b0c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Language', 1, '5030'),
           ('eff3c2d9-7a97-49f7-81e2-1b7e7876e135', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Language', 1, 'FK2B'),
           ('fedef808-6d3a-4e3b-941c-9c5fb17f940a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Language & Literature', 1, '5010'),
           ('32ead5a9-d7b2-488d-acf8-c4cecde7a371', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Literature', 1, '5110'),
           ('979af5b7-ab48-40a9-a47d-c50c0057841f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Literature', 1, 'FC4'),
           ('10261931-8acf-47c7-a5f3-4ec4987e2b43', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Literature in Dual Awd', 1, '5160'),
           ('2001af93-072d-42b9-9bfa-278b83e54370', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English Studies', 1, 'FM31'),
           ('fbe81eb9-4157-426b-a10f-6f541e498341', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'English T.A. (KS1)', 1, '9976'),
           ('3c058465-cc58-4eff-9dcb-0cc50ab770e3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Environmental Science', 1, 'QA3'),
           ('242f91b7-c4d7-4b66-ab3a-9181c8b4bd39', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Environmental Studies', 1, '3930'),
           ('c867ce1d-d4b8-4235-b2c4-91deecce9dab', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Environmental Studies Dual 1', 1, '3950'),
           ('4156806b-4b3e-476a-a0d8-0b8b343ccb18', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Environmental Studies Dual 2', 1, '3970'),
           ('71479dbe-f3ca-4bb9-ab12-dc28989b9fd6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'ESOL - Listening', 1, 'FK2L'),
           ('6d29363f-0f82-4b5c-ab07-7f58a2912eae', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'ESOL - Reading', 1, 'FK2R'),
           ('fec98cab-ac9f-49fa-a2a3-0d47b7e1431f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'ESOL - Speaking', 1, 'FK2S'),
           ('4208628c-c06d-4e47-9c03-e8f07e64b6f8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'ESOL - Writing ', 1, 'FK2W'),
           ('75f88124-2b8f-4903-8127-5383eee1a55d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Ethics (Science)', 1, 'DE3'),
           ('e9ebd219-b872-4753-87e8-fbd40ea05e00', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'European Studies', 1, '4150'),
           ('31c6a40c-f288-4c23-b433-f470a6bf9cba', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'EWTP', 1, '2990'),
           ('c6658305-c0a0-4e17-a51c-e403f4332687', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Expressive Arts', 1, '5330'),
           ('200ea144-7e83-43b7-a793-df3ff5b5f4e3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Farm Studies', 1, '7710'),
           ('e867c311-d91d-416c-ae17-ed9f09760408', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Film Studies', 1, '5360'),
           ('cbefab6d-8cd5-474e-99d7-49affe59a6af', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Fine Art', 1, '3690'),
           ('5cfe9cdc-4000-432e-8909-e5f866e36199', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Food Science/Technology', 1, 'NH'),
           ('78e4381b-c368-4b32-ba76-bb1c4c1cb797', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Food Technology', 1, 'NH6'),
           ('3865d77d-b6a5-45e7-8804-324473fe1e38', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French', 1, '5650'),
           ('d465af8a-d4d5-4a33-9c36-a72074687aa4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French (Voc)', 1, '97'),
           ('59af4d35-b623-4cc6-8c78-b0f9131d2a87', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Lang Listening', 1, 'FKFL'),
           ('b8357fce-38aa-4e94-993e-53d6304ae6ce', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Lang Reading', 1, 'FKFR'),
           ('f56d3a62-fad5-4a3c-9fec-e3cab1b28e53', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Lang Speaking', 1, 'FKFS'),
           ('68a5e6f5-f91f-4ded-92ca-560073c9eb94', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Lang Writing', 1, 'FKFW'),
           ('42fb8dd7-38bc-4062-a157-85473155a333', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Language', 1, 'FKF'),
           ('f7aeda86-aa70-490c-a6e2-0081152cf4f8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French Studies', 1, '4170'),
           ('fce40760-02bf-4010-a5c5-945bdd314c35', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'French/Business Studies', 1, '8700'),
           ('c2ce4bcb-ad76-45be-afd7-72157e35ca78', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'General Studies', 1, '7810'),
           ('fc50de31-6297-4650-9f0b-fd772cc43801', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geography', 1, '3910'),
           ('46339fd9-938e-465f-bda6-18bda14ab078', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geography', 1, 'RF4'),
           ('00e31a54-6f74-495e-9cd7-cfef2f5224b0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geography/Business Studies', 1, '8730'),
           ('1f31a41d-f195-457e-98e0-81fc499d8b3a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geography/History', 1, '8740'),
           ('0e629c34-9e88-429c-838a-3882ec1715c6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geology', 1, 'RF2'),
           ('a4fb99c3-a1ea-4a12-a5c9-d05ecc46cbf5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Geometric/Engineering Drawing', 1, '3010'),
           ('d831351a-ad46-4214-82e1-3b730b6ef1b1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German', 1, '5670'),
           ('b14e4478-1d38-4b8c-903a-ae60fa77babb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German', 1, 'FKG'),
           ('2b3599fb-45ee-45f6-a510-ba7b88eae1b1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German (Voc)', 1, '98'),
           ('200b139a-7c71-418a-a747-15dfcacedcfd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German Lang Listening', 1, 'FKGL'),
           ('e0e0e6a5-f774-429d-8285-ad8a4e4d2236', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German Lang Reading', 1, 'FKGR'),
           ('b33c9339-5ffd-49bf-b7da-ba6b179eb365', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German Lang Speaking', 1, 'FKGS'),
           ('72ceabf6-3e84-408f-9cce-e44d14a7e03a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German Lang Writing', 1, 'FKGW'),
           ('aeb925c7-c0ca-48cd-a91f-05b39bbfff1f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German Studies', 1, '4270'),
           ('6d13f351-72b6-45bd-9585-bb4c1720c6bc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'German/Business Studies', 1, '8710'),
           ('e6401212-47fc-4bf2-a83a-3444801abb73', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Government/Politics Studies', 1, 'EA'),
           ('96e2c754-d52a-4752-a9a8-38bdcac35abb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Graphics', 1, '3030'),
           ('4ed0b3d5-1f95-4383-bc65-51383d2d6926', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek', 1, '6550'),
           ('d33c758f-b4bb-4654-b074-9b6d132d0157', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek', 1, 'FKK'),
           ('7a8d31c7-c7d9-4cbe-b897-0d89cad43dab', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek (Classic)', 1, 'F1K'),
           ('ad597dfe-3806-4fbb-a938-f7468ff83990', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek/Classical Studies', 1, '6570'),
           ('29419904-5b41-4e32-a6d2-5794eb8ae51d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Greek/Roman Civilisation', 1, '6590'),
           ('2290bdf7-68be-4c63-a760-89fd29b82b0e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Gujarati', 1, '5970'),
           ('1ec9151e-6eb6-477e-8e77-00519a5b3623', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Gujerati', 1, 'FKI'),
           ('f4a00104-84b5-4196-b8cd-a9d19d62eda7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hairdressing', 1, '18'),
           ('111f3501-f271-4f53-9ecf-720c727a92f0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Health & Social Care', 1, '3'),
           ('3146bfe4-a023-4580-a208-1ac808cd269c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Health Studies', 1, 'PA1'),
           ('61ec9771-4d0f-4806-a1a9-6eba3d015437', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hebrew Language', 1, 'F1H'),
           ('6d2a8702-9b9d-4e92-bf88-4d3db5b6a041', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hindi', 1, '5990'),
           ('1a77b7db-fc04-4602-bfa6-15169ea19d71', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hindi', 1, 'FKH'),
           ('5749016e-5be2-4672-8f5d-b488cfbc857e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'History', 1, '4010'),
           ('7eec1469-5841-46f2-bd6f-3aa08ede80bc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'History', 1, 'DB'),
           ('8848a225-964a-46be-9673-a1abcd842146', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'History of Art', 1, '3830'),
           ('7cae4180-7483-4a31-aefe-525316e6d66c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'History of Music', 1, '7030'),
           ('3b87303e-bbb3-4ec5-8ea5-7927160b8abd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics', 1, '3310'),
           ('16bac9fa-d733-4129-8008-771c9d2a30da', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics', 1, 'NG'),
           ('0bdfcb2d-b030-4f5f-bb28-930581098133', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics: Child Devt', 1, '3330'),
           ('d0500335-767a-4bf4-b951-7e78faa3a673', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics: Consumer Studs', 1, '3320'),
           ('0b7dfddb-d78e-4c52-874f-2b10d346d25b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics: Food', 1, '3350'),
           ('9923abc2-c8d7-493e-8f89-e4621990e2fa', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics: Home & Family', 1, '3390'),
           ('a087e618-4f67-4843-97cd-44a0cd09d2e5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Home Economics: Textiles', 1, '3370'),
           ('371b27ad-140d-4ecd-ba70-518f6de16f96', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hospitality & Catering', 1, '7'),
           ('649399f9-909e-4773-bde1-17025fadd8ba', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Human Biology', 1, 'RH41'),
           ('a53f7ddc-fe55-4f9f-84c3-0451e4798026', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Humanities', 1, '4510'),
           ('e282f48f-e23c-4d89-b4f1-073499f5dc23', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Humanities', 1, 'DA'),
           ('c77bba7b-10f1-4faa-bd0f-8fa7531c45de', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Humanities Dual Award 1', 1, '4530'),
           ('bb4dce4e-eea8-468d-ac67-86931af220f4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Humanities Dual Award 2', 1, '4550'),
           ('67888169-f3d3-427a-867d-58b6eb279fc9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Hypothesis Testing', 1, 'RB7I'),
           ('5e303788-c6a0-4ee5-bb98-d6b5559250cb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Industrial Studies', 1, '7470'),
           ('ad5440d4-e24d-4b07-96b6-9915b70827ec', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Information Studies', 1, '2670'),
           ('476294ac-a102-47d5-9b1c-356d5f2c4d2c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Information Systems', 1, '8200'),
           ('436e6fca-c40e-431a-ad7c-cf074ca39eb9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Information Technology', 1, '2650'),
           ('0a3fc394-e977-48c6-bd8c-a802c690f085', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Information Technology (Voc)', 1, '10'),
           ('838777ec-4f14-431f-b83b-bc849d862f85', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Integrated Humanities (sgl)', 1, '4520'),
           ('55122aa7-8724-45c3-be39-0c5eb4aeb399', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish', 1, '5550'),
           ('cb63de69-e133-42a4-ac4f-6a6addb28a2d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish', 1, 'FKE'),
           ('5cd9f6d3-d31b-498a-a549-108e8439bd93', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish Gaelic Listening', 1, 'FKEL'),
           ('e99138e1-f068-42d1-8ced-419f8ff4aec8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish Gaelic Reading', 1, 'FKER'),
           ('44ababa0-0b7b-4d61-ac79-69bb0ffa3aa2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish Gaelic Speaking', 1, 'FKES'),
           ('5e8ce63e-b311-42b9-ac60-7931f1b3269e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Irish Gaelic Writing', 1, 'FKEW'),
           ('8f514bd0-8c43-4ea1-b789-e924b3b57eac', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Art', 1, '8210'),
           ('5537babc-29fe-4d5f-a0b0-cd5f6dad6e68', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Automotive Engineering', 1, '8220'),
           ('afe697ed-1d4b-4864-9a49-44fc0977e2d0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Buillding Studies', 1, '8230'),
           ('84500bde-284a-4f24-affa-6ba972920caa', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Business Studies', 1, '8240'),
           ('aba364fd-0175-43c8-b655-f612b1db1e1f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Catering', 1, '8250'),
           ('46fdfd03-962d-4608-923f-06163638a433', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Drama', 1, '8260'),
           ('002acdab-95ca-4a31-a8d5-5013d108f979', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Economics', 1, '8270'),
           ('0d467685-66a2-45f6-9186-a2f6028e5fc7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Electronics', 1, '8280'),
           ('93d237a4-5ddf-4e37-9016-1c02890bc9ac', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Fashion', 1, '8290'),
           ('c182d9e9-ac47-41d9-9a11-eeb5cc691f79', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Food Industries', 1, '8300'),
           ('20ec1cf8-9362-415c-baed-00ff9dbff9d6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Health', 1, '8310'),
           ('634589e6-38b4-40e5-93fe-45fdb3a8cb19', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Industry', 1, '8320'),
           ('9105416f-4339-4f8f-af1f-42b2157850e7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Music', 1, '8330'),
           ('0e921d50-c0ef-4b42-973b-4870c513e9d4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'IT/Transport', 1, '8340'),
           ('f5e65154-bcb4-4c08-a55c-03d707cd5491', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian', 1, '5690'),
           ('4c00959c-17bd-414e-8ffd-a129a0b8d461', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian', 1, 'FKX'),
           ('2515a352-88b2-4c0a-90b1-b168267dd1e5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian Lang Listening', 1, 'FKXL'),
           ('f8454670-98ea-4803-9622-d708f6f18540', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian Lang Reading', 1, 'FKXR'),
           ('8f3b4b3f-6d89-4a01-b432-984f9e69a5f9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian Lang Speaking', 1, 'FKXS'),
           ('58a5bbfe-1664-489e-adaa-5ee8f3a6b32d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Italian Lang Writing', 1, 'FKXW'),
           ('bf8d593e-6c35-4fc9-abc5-329cb0d1f6b4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese', 1, '6010'),
           ('784f4d10-b2cb-409a-8a0a-2ae25590bc48', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese Language', 1, 'FKJ'),
           ('8863e756-01eb-4f5b-b023-ed136dd0642b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese Lng Reading', 1, 'FKJR'),
           ('fe08ad07-419c-4538-8c35-8bd200edb772', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese Lng Speaking', 1, 'FKJS'),
           ('4c1b887d-a252-4ae6-a95c-be6280917dcd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Japanese Lng Writing', 1, 'FKJW'),
           ('bc0b75dc-f75f-4da8-b9f9-f65be98e856f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Jewellery', 1, '6850'),
           ('f7fd1c16-08fb-4c99-9ffa-75e91cd26866', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Job seeking Skills', 1, '19'),
           ('0d6fa7d2-68a1-40e7-9f24-b1d4501089c4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Journalism (General)', 1, 'KD1'),
           ('e28de837-971d-473c-8f49-42457875b37d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Key Skills', 1, '9900'),
           ('87dc0cf1-e852-4ebc-9fe5-1356fe207fd5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Keyboarding Application', 1, '7490'),
           ('42a85bf0-f9a2-4e4d-857a-1dc2b1f9fa08', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Application of Number', 1, '9961'),
           ('4f4fbcd9-925c-44f2-a3da-cae5d3d3bbc4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Communication', 1, '9962'),
           ('0608eb09-231f-4c86-b0f4-794a7eb73404', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Improving Own Learning', 1, '9960'),
           ('63415c54-c6ab-4b82-a151-9fbbd5cf9818', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Information Technology', 1, '9963'),
           ('35747274-d3c9-43c4-a4b6-19a7e2d14d87', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Problem Solving', 1, '9959'),
           ('b25392ce-5525-498a-9a13-4ba58ec72285', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'KS Working with Others', 1, '9958'),
           ('31cbf820-8512-4813-8922-58627ef1b4af', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Land & Environment', 1, '14'),
           ('8cd7be83-9794-4506-8b1d-f31aef1812d9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Latin', 1, '6610'),
           ('fe38f1dd-6ab0-4654-900b-88abf2f8fc83', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Latin', 1, 'F1L'),
           ('9f6d9c83-e271-4dbf-bd27-363f16bc4f6b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Latin Literature', 1, 'FC6L'),
           ('7d8b8203-bddf-483a-ba23-bfb11b462fa6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Law', 1, '4770'),
           ('ba28baac-077f-4ee6-a7a4-be235e760622', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Law/Legal Studies', 1, 'EC1'),
           ('f363af8c-cf55-4a2c-b88e-8e1258805d8d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Learning Skills', 1, '20'),
           ('b9fdd329-6c91-4c88-991e-fa08d5cb4368', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Learning theory', 1, 'GA33'),
           ('96d7b739-371a-464f-bc45-acfe9b5e0484', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Leisure & Tourism', 1, '4'),
           ('39c5f26e-17c9-4a6d-a9c7-89143c8bf8c2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Leisure and Recreation', 1, '16'),
           ('43983f4e-87be-4460-91ce-ec261798cd46', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Library Studies', 1, '7510'),
           ('8d5ae7dd-28e1-4f08-8cec-b33998179e80', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Logic (Philosophy)', 1, 'DE51'),
           ('2d74a816-eda6-4084-a067-6a16c8cf5b49', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Logic/Philosophy', 1, '4790'),
           ('3b5a331b-907c-4850-9fcc-c2c4fea61d05', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Management Studies', 1, '11'),
           ('26283a20-545b-4526-ac32-ecc330fb92ee', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Manufacturing', 1, '5'),
           ('d5ed34ec-f5ad-4f25-b6c9-09134bf1fc1c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Manufacturing Technology', 1, 'WA2'),
           ('81264e74-12d2-4e1d-98a7-1e0cb8e5c5d7', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Marketing', 1, '7670'),
           ('57ca1785-0269-4baa-bb10-7e6d73092fb5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics', 1, '2210'),
           ('d124ccc5-d4cf-4bf9-8200-7a7c1d0a475c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics (KS1)', 1, '9975'),
           ('7d7fc0db-8e8c-4f0e-a197-bd969ba031de', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics (KS2)', 1, '9982'),
           ('71cf4557-f43a-472f-b392-b2155891b406', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics (KS3)', 1, '9992'),
           ('7a652bad-2668-406e-96b3-fd41b20da450', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics (Mechanics)', 1, '2220'),
           ('6fa765f3-6654-4480-be7a-4dc9dc01fafa', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics (Statistics)', 1, '2260'),
           ('cafa6f99-a35d-4c25-931a-6f2fd3e1501e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Additional', 1, '2340'),
           ('2a07f118-64c1-4a11-870b-9bd7416d12f1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Applied', 1, '2250'),
           ('74ff35c1-4700-4dc8-aee4-682ca49e9999', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Decision/Discrete', 1, '2240'),
           ('b7d78af6-2570-4418-86ce-0a951498e11a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Further', 1, '2330'),
           ('a967caa5-a50a-4b7b-9167-235a180cf6ee', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Pure', 1, '2230'),
           ('66efa159-0287-4d27-ad40-47cc3bd9a548', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Pure & Decision', 1, '2280'),
           ('1d331f36-8ff2-46ff-8a24-f6f226f88ad4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Pure/Applied', 1, '2270'),
           ('b94e3aa2-3ff1-4166-9207-21627573e2a2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Pure/Mechanics', 1, '2310'),
           ('868b67b6-5e61-476a-979d-e73d1391e505', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Pure/Stats', 1, '2290'),
           ('6316f21d-e354-41b3-b900-832bb9d727fc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Mathematics Studies', 1, '2350'),
           ('cd9d0102-be9e-4667-8317-f843f2c0fd90', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Maths (Further)', 1, 'RB1A'),
           ('5eda2d38-ec16-47b8-8b87-1061001f6505', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Maths (General)', 1, 'RB1'),
           ('506f35b4-9814-4a93-9594-2d8c1e778064', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Maths Principles for Personal Finance', 1, 'RB7B'),
           ('fa57a6ca-7346-4d62-82c9-9b27e943bb60', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Media Film and TV Studies', 1, '5350'),
           ('c38ce1b5-8588-42b5-8e04-a51caa2555d1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Media Studies', 1, 'KA2'),
           ('b8124e9b-4997-412c-941e-1bdd5b283e43', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Media: Communicn & Production', 1, '12'),
           ('0112b6b4-6150-4fe4-951f-d42549800af5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Metalwork', 1, '3050'),
           ('dcb9e8ce-5487-4b51-b931-bc41feb58cc6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Method in Maths', 1, 'RB1G'),
           ('7798ab1e-f7fb-45de-b4f9-9179d480231a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Modern Greek', 1, '5710'),
           ('3e36457e-d59b-4fe5-bf2c-38cc9fa515a6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Modern Hebrew', 1, '6030'),
           ('b1c17529-8242-4495-a54a-faf97d7f7533', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Modern Languages', 1, 'FK12'),
           ('1dc8a2bd-ff76-4cfe-98f4-b13626604412', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Money Management', 1, '7530'),
           ('9a739de9-8c78-491b-a25c-9cbfdfde8e2c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Motor Vehicle Studies', 1, '3070'),
           ('2f5c85b3-6c0d-4b8e-aee1-a92cc67ff4aa', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Museum Studies', 1, '4190'),
           ('f88a4cb2-453a-4426-bab3-d52bcd7f25cf', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Music', 1, '7010'),
           ('a29c8b90-dbb5-4920-ace1-9de84ec070c0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Music - Practical', 1, '7020'),
           ('3655fccb-6501-4e9c-ae2c-0e9988790f67', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Music Technology', 1, '7040'),
           ('1c5eaa24-ba52-4686-898a-0d86ce2a000c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Nautical Studies', 1, '7550'),
           ('1898e91e-7c54-4da2-95f1-2fbba9beb0ca', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Navigation/Marine Navigation', 1, '7560'),
           ('467ab513-61f3-4644-b4fa-de0936d2add8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Numeracy', 1, 'HD4'),
           ('7dd10087-8ea7-4213-9bc8-0e909289a0cf', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Nursing', 1, '7690'),
           ('f2a50c4a-c68a-4051-bd01-36477e8a1e8b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Office Technology', 1, '7460'),
           ('4595e1a9-92ec-4205-a975-3ac3f4043fea', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Classical Languages', 1, '6650'),
           ('e8f38513-3820-47d6-ab31-64b9935b142c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Classical Languages', 1, 'F1Z'),
           ('9ae29f86-bb92-4e88-93a3-90888f541e69', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Other Languages', 1, '6310'),
           ('f249ec9a-3865-45d9-bc85-7080be4a795f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Outdoor pursuits', 1, '7250'),
           ('7a562e20-78de-4d83-bb4e-cc5effa3d3ec', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Performing Arts (Voc)', 1, '15'),
           ('cf4e6ea2-73a2-4d5e-a97b-3fae58b07845', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Persian', 1, '6150'),
           ('e9810a08-c6b1-4cdc-a61a-d71d31282adb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Persian Language', 1, 'F1P'),
           ('c54d5c3c-0771-42e7-ab7a-d9b4aa550c32', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Personal and Social Education', 1, '4810'),
           ('71eace0a-5ae7-4476-9f70-c4e140016c5e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Philosophy (Theory)', 1, 'DE1'),
           ('320e1743-0d0f-44d4-b30a-90ef60c21637', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Photography', 1, '7570'),
           ('aa2dd40e-89e3-42a6-a13a-06682f7b411d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Physics', 1, '1210'),
           ('782ab6eb-60b3-4032-82e6-004f56946c3c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Physics (General)', 1, 'RC1'),
           ('a324fe5e-9625-47c5-ba91-a5b7581ff5b0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Planning and Maintaining', 1, 'TA3'),
           ('5cc547f4-0ab7-4a2a-b4c6-8b33613014df', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Polish', 1, '6070'),
           ('17c3d82b-e766-46d6-8f08-9ef7772cb49b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Polish', 1, 'FKO'),
           ('2ff61aa5-914a-4915-85ef-aebd863614ac', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Politics', 1, '4830'),
           ('98521810-cdd4-488b-a306-9c9cd25aec1b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Portuguese', 1, '5730'),
           ('f982f2a6-49ec-4cc8-9f7e-424ae80cb0bc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Portuguese', 1, 'FKP'),
           ('2c6344d3-f40f-410b-aba0-e39b5041b630', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Preparation for Employment', 1, '21'),
           ('21f1ec19-86a0-4b0d-b8ee-d5cc23b8f85b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Preparation for Work', 1, 'HC42'),
           ('fcb3b2ac-2b69-4c81-b60a-2d6f1a65f9d6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'PSE Dual', 1, '4820'),
           ('bc7e4e81-342f-40d4-8b30-b6996a0c4c53', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Psychology', 1, '4850'),
           ('a580a0db-2cc7-4d29-b373-62bf8e23a873', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Psychology (General)', 1, 'PK1'),
           ('0e333218-8ade-42a7-b54a-80120cd97896', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Psychology (Science)', 1, '1830'),
           ('439a7e23-8348-4322-9114-67935ffa489f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Public Affairs', 1, '4870'),
           ('381dc7a0-8527-4119-8c1c-8a97342c285d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Punjabi', 1, '6050'),
           ('7d457a9a-a94f-4af8-8ece-c806cd68a7c6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Punjabi', 1, 'FKQ'),
           ('0b58b68f-344b-4bae-aa69-2bea41ba140d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Pure Maths', 1, 'RB15'),
           ('96c0cbe5-29bd-48c0-b2e4-5063d680cc96', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Reading Comp.(KS1)', 1, '9972'),
           ('547834ab-c00a-4913-b106-6188e7a2c4e0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Reading Task (KS1)', 1, '9971'),
           ('f43ae6e5-b30a-4a6d-ae51-a6b9d5f2c22c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Religious education', 1, '4630'),
           ('d25ccd1e-24da-46ad-91c2-0bdbcee114cb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Religious Studies', 1, '4610'),
           ('ea4cab9b-ed51-4bd5-bc63-901313e0ecfe', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Religious Studies', 1, 'DD1'),
           ('012b1a1e-c43f-4473-a6f1-01ac693868d2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Retail & Distribution', 1, '13'),
           ('2c74fae5-fbd2-411c-89c4-a599a7c1772b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Road Safety', 1, '7590'),
           ('691e54b3-932c-4e1b-b5e5-7cf2fed19cf9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Road Vehicle Engineering', 1, 'XR'),
           ('66613d09-a67b-42ba-b5f7-c076f903b8ce', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Robotics', 1, '1850'),
           ('630ccfda-5480-441a-aebd-f25c8609f587', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Roman/Latin Civilisation', 1, '6630'),
           ('c399bc92-128d-4364-88a9-4d938351460f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Russian', 1, '6090'),
           ('fedebe64-b940-414e-b3af-3f06d0c87105', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Russian', 1, 'FKR'),
           ('db7ffc3e-1d05-447a-abd1-225085ecea25', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Russian Studies', 1, '4210'),
           ('b663eb2a-04e6-40e7-95be-fe16ab7190e8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science (General/Combined)', 1, 'RA1B'),
           ('bfad31fd-4317-4c40-8965-311098d7dcd4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science (KS2)', 1, '9983'),
           ('67d70aef-3e68-471a-8145-0e7822c81169', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science (KS3)', 1, '9993'),
           ('cdc7b3ff-9d6d-4f6c-a1f8-2305a9f86858', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science (Voc)', 1, '8'),
           ('65ede278-27b1-4136-919d-48664219e6a5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Double Award', 1, 'RA1E'),
           ('0b7cc301-0b8e-4e35-a73f-7c3fe2a6cefc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Double Award 1st Subj', 1, '1370'),
           ('b3a25b61-8546-4703-9837-fb3761ec03f2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Double Award 2nd Subj', 1, '1390'),
           ('734cd42e-cb70-4adf-901a-245672e20614', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Dual Award 1st Subj', 1, '1330'),
           ('cccdd775-f94a-4bc5-88ae-5ac2c6f7212c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Dual Award 2nd Subj', 1, '1350'),
           ('7f9934da-e530-43c8-8e11-48f290b4d598', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science for Public Understanding', 1, '1920'),
           ('bae59149-6c3f-4992-93d4-487589cf08dc', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science in Society', 1, '1910'),
           ('ec68bd0d-f0ac-4737-b4d0-ceafc379208a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science Single Award', 1, '1310'),
           ('14df3a1c-04cf-4d24-be64-89d2633ab786', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science T.A. (KS1)', 1, '9977'),
           ('7ae1bafa-e9c3-473b-b117-b4ea44a153da', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Additional', 1, '1320'),
           ('61dd682e-4230-4663-85bd-e0fe609633a8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Aeronautics', 1, '1630'),
           ('cae77c0f-02d6-4840-9e21-24c5a9fa0aea', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Agricultural', 1, '1650'),
           ('5e76380c-1572-47f9-9163-5894ec70c54e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Agriculture & Horticult', 1, '1660'),
           ('ed95d607-1f0f-4bdb-8e51-6d93926266a3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Applied', 1, '1670'),
           ('e2846c66-f55b-4264-a6b5-4db484b992df', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Astronomy', 1, '1690'),
           ('3c5d18ec-b4da-4b2d-b034-e0b7b8281557', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Biology and Chemistry', 1, '1410'),
           ('b009b8e6-2038-474a-9006-bf727f85c30c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Biology and Physics', 1, '1450'),
           ('9379c14e-2244-4836-a00e-9c160439ddeb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Botany', 1, '1710'),
           ('eaca221b-9f25-4c9a-a344-64dc29e5c068', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Chemistry and Physics', 1, '1470'),
           ('0c3e7f83-0065-4ddc-98e7-49e393c06eec', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Electronics', 1, '1730'),
           ('53f6054b-ab8f-4754-8a2b-74a3cfe24a7c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Engineering', 1, '2010'),
           ('7e591b91-3d59-4bc8-8594-e988e9426a42', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Environmental', 1, '1750'),
           ('fbcd11a5-d956-472b-9e86-39b532c76333', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Geology', 1, '1770'),
           ('c03f5a96-d7ed-454a-9606-23e13e9938fe', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Horticultural', 1, '1790'),
           ('5d022242-d23b-4877-8088-e1482816a491', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Meteorology', 1, '1990'),
           ('3ddb9784-996f-42ad-b572-e23a237d3f78', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Other', 1, '2030'),
           ('533f2589-f515-4397-83d6-019a7887a520', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Physical', 1, '1810'),
           ('dc79e850-6172-4d10-bad2-7ace062974e8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Rural', 1, '1870'),
           ('79cd9c6b-d733-4ff1-b778-184c8f60e188', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Technology', 1, '1930'),
           ('f5e71573-c58e-4542-9e58-2780297256a4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Science: Zoology', 1, '1970'),
           ('568640c4-c9fa-4c23-b85b-67519babbcda', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Social Geography', 1, 'RF44'),
           ('a3e405c9-2c2b-405f-8748-f31078293c89', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Social policy', 1, '4895'),
           ('532e5080-6d36-45fd-8bbc-aef7ecbdbcb4', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Social Science', 1, '4710'),
           ('d26f8957-3690-4001-a4a8-b24bdbf9e012', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Sociology', 1, '4890'),
           ('a36831a4-c25e-4c09-9de4-f5baf2d64dc2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Sociology', 1, 'EE2'),
           ('0b1d111a-250a-401b-b72a-dec01397571c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Sound Recording', 1, '3090'),
           ('f74f2f7f-8cf1-4102-8224-fdc3f67e4b8c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish', 1, '5750'),
           ('64631fe8-6088-48f2-80f3-18b52d56f9bb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish', 1, 'FKS'),
           ('c164d28c-d1ed-44e7-be5f-eeb3bf46cddb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish (Voc)', 1, '99'),
           ('1192c269-c854-4dcd-9024-7f706b29cfef', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish Lang Listening', 1, 'FKSL'),
           ('3ecdcf63-3e37-4882-8139-6ccb6b24bec1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish Lang Reading', 1, 'FKSR'),
           ('92a337df-58e0-4ec4-a2ca-51b633939e3a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish Lang Speaking', 1, 'FKSS'),
           ('9b2ff0b9-0fc9-4f3f-9ab4-e109352de158', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish Lang Writing', 1, 'FKSW'),
           ('ba3351e6-ba57-4c28-8903-df4ac8479a7b', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish Studies', 1, '4230'),
           ('bcced548-b294-48a9-8fe1-03395c002298', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spanish/Business Studies', 1, '8720'),
           ('58c94809-57c1-4533-9ab8-afe083416c81', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Spelling (KS1)', 1, '9974'),
           ('1e03d76b-8137-46b8-8e7d-e899839c1d45', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Sport/PE Studies', 1, '7210'),
           ('7a09b11b-9965-4a5d-bcca-2d692ded9774', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Stage & Perf. Arts Dual', 1, '5340'),
           ('b57a2972-86a6-498f-9ace-0ec4d81d21f3', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Statistical Analysis', 1, 'RB7F'),
           ('206c6375-a0fa-426f-a39d-8383e88dcfe6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Statistics', 1, '2510'),
           ('6dc1a334-e57d-44c9-92d1-636b9c7b6b12', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Statistics', 1, 'RB71'),
           ('cf3ec685-85da-4794-9e3b-2e197cc4c256', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Statistics & Decision Maths', 1, '2300'),
           ('8d9e22dc-eb32-467c-a228-e075ebb2dc38', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Surveying', 1, '7610'),
           ('b7434895-edf5-4e9e-8d96-ab438c717235', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technical Studies', 1, '3130'),
           ('57288922-6761-4dce-9e3b-7125f7348d71', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology', 1, '8400'),
           ('81a70cfd-9a80-4190-9a14-bb3fdbe7c9bb', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Art', 1, '8410'),
           ('a3c1fb22-7a00-4ee1-99d4-961a8e4a7c39', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Automotive Engineer', 1, '8420'),
           ('6a5792b9-5dd4-4be8-af36-09b91269db41', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Building Studies', 1, '8430'),
           ('22afac5f-c0d1-44ce-ba1d-6874adb8e30d', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Business Studies', 1, '8440'),
           ('059def57-1b0e-4afe-bf00-93e690453e71', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Catering', 1, '8450'),
           ('0ee1de5a-c87a-4268-9ad7-7fbea111be26', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Construction', 1, '8460'),
           ('558c4d31-9ea8-42c7-9240-623a11462850', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Drama', 1, '8470'),
           ('e051d608-8a4e-4fd7-985d-543da164dedf', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Economics', 1, '8480'),
           ('a622d018-c92d-4880-874e-ab1925e2c6be', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Electronics', 1, '8490'),
           ('dc8f8b2a-ad42-4bbe-8ee6-56c1e4a9a19f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Fashion', 1, '8500'),
           ('341f6f51-607e-49a7-b491-148f253b917f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Food Industries', 1, '8510'),
           ('7f8f01a5-e8ce-4207-9ac0-a21273d94228', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Industry', 1, '8520'),
           ('835530bb-9baf-4847-8384-fbf9fcaf1e68', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Music', 1, '8530'),
           ('57d27817-ac6b-4290-9686-3b7343cb292a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Technology/Transport', 1, '8540'),
           ('3a7fcf22-58a2-4d46-9994-c1a85f21e904', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Textile/Fashion Shows', 1, '7630'),
           ('b398c657-76f1-44c0-84e3-9b582c13edf9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Textiles/Fabrics (Industrial)', 1, 'WH'),
           ('0d721483-696c-4bc3-b2ce-2586d0b5d93c', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Theatre Studies', 1, '5370'),
           ('6d4e71cf-a5c1-4cab-b00e-4cb8524e1c19', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Tourism', 1, '7650'),
           ('3f7d997b-e530-4f8f-a3c3-753ff66873af', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Travel & tourism', 1, '17'),
           ('82fa5d5b-e163-4067-ad77-ca508d917977', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Turkish', 1, '6110'),
           ('e9831c8e-7443-41d9-9710-c4506139dede', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Turkish', 1, 'FKT'),
           ('87b35da2-3634-49fe-ab67-71ded7f4b5d1', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Urdu', 1, '6130'),
           ('74c9aac8-c1fb-4000-babe-cba7c24964c8', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Urdu', 1, 'FKU'),
           ('aafb61bd-6229-4112-942b-e18eef562c7e', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Use of Mathematics', 1, '2215'),
           ('983fedea-fc3c-4b50-b699-058ac4fb7f9a', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Use of Maths', 1, 'RB7H'),
           ('bcbdfa40-ef7b-477f-93ac-ae5a9a19ef7f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Using & Applying Decision Maths', 1, 'RB6A'),
           ('636e4b34-f5b6-41ec-923a-1124660d0261', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Using & Applying Statistics', 1, 'RB7E'),
           ('9f8f4e89-e51b-48d2-90d7-a409e474eed0', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh', 1, 'FKW'),
           ('911fbb0c-c7fb-42d1-a138-005467a35e1f', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (2)/Art', 1, '8600'),
           ('4c91947d-5a88-46da-b5bb-cd65877b32d9', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (2)/Business Studies', 1, '8610'),
           ('e5511405-9c99-49e7-b466-770e1aa94dc6', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (2)/Drama', 1, '8620'),
           ('09945840-4bf4-47ee-afec-4630c14f90da', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (2)/Information Technol.', 1, '8630'),
           ('dd747b44-71fb-41be-83a2-726424633461', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (2)/Religious Studies', 1, '8640'),
           ('7cf95269-bfa4-494a-807d-7c987c61bf82', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh (Second Language)', 1, '5515'),
           ('8753c3f2-a5b9-4e06-a53f-73953390d6d2', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Lang Listening', 1, 'FKWL'),
           ('d3458584-8c73-48cc-80f3-875614459a00', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Lang Reading', 1, 'FKWR'),
           ('83d91801-1b09-415f-8b8e-88a3ab54bc33', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Lang Speaking', 1, 'FKWS'),
           ('2b411365-1188-4ae2-8fbb-3d81c4c75d00', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Lang Writing', 1, 'FKWW'),
           ('bb14031c-2366-4028-8df8-c18d71185061', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Language', 1, '5510'),
           ('224bbca0-f55a-4db1-8cbd-db30b5b995dd', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Literature', 1, '5530'),
           ('98e43ce3-591f-4248-a404-d31b5b382adf', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Literature', 1, 'FC62'),
           ('5ee6e160-f07c-477c-90ea-37070b27a753', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Welsh Studies', 1, '4250'),
           ('272042d9-caa8-4300-9af1-7ec270b0b1c5', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Woodwork', 1, '3110'),
           ('75eb96cf-2daf-4066-9048-7e4cd1902389', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'World development', 1, '3920'),
           ('7a17aeeb-81e5-4229-87ed-90af8e053c07', '3A5F93CA-E082-4EAA-96D7-C538F723502B', 'Writing (KS1)', 1, '9973')
) AS Source (Id, SubjectCodeSetId, Description, Active, Code)
ON Target.Id = Source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, SubjectCodeSetId, Description, Active, Code)
    VALUES (Id, SubjectCodeSetId, Description, Active, Code);

MERGE INTO [dbo].[ExclusionTypes] AS Target
USING (VALUES
           ('8BE7A245-E1BB-44DC-B427-0247D9CEA9AB', 'FIXD', 'Fixed Term', 1, 1),
           ('8BE7A245-E1BB-44DC-B427-0247D9CEA9AC', 'PERM', 'Permanent', 1, 1)
)
    AS Source (Id, Code, Description, Active, System)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Code, Description, Active, System)
    VALUES (Id, Code, Description, Active, System);

MERGE INTO [dbo].[ExclusionReasons] AS Target
USING (VALUES
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35000', 'PP', 'Physical assault on pupil', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35001', 'PA', 'Physical assault on adult', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35003', 'VP', 'Verbal abuse/threat against pupil', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35004', 'VA', 'Verbal abuse/threat against adult', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35005', 'OW', 'Use/threat of use of an offensive weapon/prohibited item', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35006', 'BU', 'Bullying', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35007', 'RA', 'Racist abuse', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35008', 'LG', 'Abuse against sexual orientation/gender identity', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35009', 'DS', 'Abuse relating to disability', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500A', 'SM', 'Sexual misconduct', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500B', 'DA', 'Drug and alcohol related', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500C', 'DM', 'Damage', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500D', 'TH', 'Theft', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500E', 'DB', 'Persistent disruptive behaviour', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C3500F', 'MT', 'Inappropriate use of social media/online technology', 1, 1),
           ('8D4324AA-A0FC-41A4-B9AE-6345D7C35010', 'PH', 'Wilful and repeated transgression of protective measures in place to protect public health', 1, 1)
)
    AS Source (Id, Code, Description, Active, System)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, System)
    VALUES (Id, Description, Active, System);

MERGE INTO [dbo].[AgencyTypes] AS Target
USING (VALUES
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D841', 'Transport', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D842', 'Catering', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D843', 'Social Services', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D844', 'Local Authority', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D845', 'Teacher Agency', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D846', 'Medical', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D847', 'Agency', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D848', 'Supply Agency', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D849', 'Educational Provider', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D84A', 'Workplace Provider', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D84B', 'Healthcare', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D84C', 'Technical Support', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D84D', 'Law Firm', 1),
           ('7B32B95C-082C-4DE9-8050-A6DF83F6D84E', 'Housekeeping', 1)
)
    AS Source (Id, Description, Active)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[AgentTypes] AS Target
USING (VALUES
           ('6DF2A876-A742-421D-824F-CBB6966AE212', 'Administrator', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE213', 'Audiometrist', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE214', 'Doctor', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE215', 'Education Welfare Officer', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE216', 'Educational Psychologist', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE217', 'External Specialist', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE218', 'Instructor', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE219', 'Physiotherapist', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE21A', 'Social Services', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE21B', 'Speech Therapist', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE21C', 'Supply Teacher', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE21D', 'Technician', 1),
           ('6DF2A876-A742-421D-824F-CBB6966AE21E', 'Tutor', 1)
)
    AS Source (Id, Description, Active)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active)
    VALUES (Id, Description, Active);

MERGE INTO [dbo].[BehaviourRoleTypes] AS Target
USING (VALUES
           ('3c487bc5-110a-4619-9ab3-be4005f2d1ef', 'Aggressor', 1, 1),
           ('83c30d68-1b9e-4e3d-94ac-525209713a8b', 'Participant', 1, 1),
           ('1ec5b055-515b-48c0-8afa-ef4320bef310', 'Witness', 1, 0),
           ('0336da30-3bc4-4582-8859-a1d4f35b5053', 'Target', 1, 0)
           )
AS Source (Id, Description, Active, DefaultPoints)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
INSERT (Id, Description, Active, DefaultPoints)
VALUES (Id, Description, Active, DefaultPoints);

MERGE INTO [dbo].[BoarderStatus] as Target
USING (VALUES
           ('4dbc575c-a035-4409-917a-3a116d51258f', 'Boarder', 1, 'B'),
           ('83b4978a-702c-4e58-9f87-c955a138dc61', 'Boarder - Night not Specified', 1, 'N'),
           ('9abf7871-f189-4a99-a7a5-e29aeab40772', 'Not a Boarder', 1, 'X')
           )
AS Source (Id, Description, Active, Code)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
INSERT (Id, Description, Active, Code)
VALUES (Id, Description, Active, Code);

MERGE INTO [dbo].[StaffAbsenceTypes] as Target
USING (VALUES
           ('6bdf1ac6-63de-4f2c-a3ca-1ef69eda2b90', 'Illness', 1, 1, 1),
           ('1c0bb33a-d5a3-4e40-8c4f-9ac29f95be1a', 'Holiday', 1, 1, 1),
           ('2e0c6c0f-4ea0-40dc-bac3-19de84327454', 'Jury Service', 1, 1, 1),
           ('33c1ac6b-7ff4-4f3e-88d5-df8f8c0d2c66', 'Compassionate Leave', 1, 1, 1),
           ('171f0b15-56a7-4420-971c-81342cb737e8', 'Unknown', 1, 1, 0)
           )
AS Source (Id, Description, Active, System, Authorised)
ON Target.Id = Source.Id

WHEN NOT MATCHED THEN
INSERT (Id, Description, Active, Authorised)
VALUES (Id, Description, Active, Authorised);

EXEC sp_MSforeachtable @command1="print '?'", @command2="ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";

IF(@@ERROR > 0)
BEGIN
    ROLLBACK TRANSACTION;
END
ELSE
BEGIN
   COMMIT TRANSACTION;
END