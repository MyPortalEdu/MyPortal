-- ============================================================================
-- Seed pupil-domain lookup catalogues from the DfE Common Basic Data Set (CBDS),
-- version May 2026 — the sets deferred from 20260626000300 because their sheet
-- rows carry note rows and embedded footnotes that need cleaning (skip / strip).
-- Same contract as 300: MERGE upserts the official rows (Active = 1), then any
-- row NOT in the set is deactivated (Active = 0) so existing FK references stay
-- intact. Ids are deterministic = md5(table + '|' + naturalKey).
--
-- SenProvisionTypes is intentionally NOT reseeded: the CBDS "Pupil SEN Provision"
-- set is SEN *stage* (No SEN / SEN support / EHC plan), which maps to our
-- SenStatus table, not to the provision-service catalogue this table holds.
-- ============================================================================

-- EnrolmentStatus (7 rows) ---------------------------------------------
MERGE INTO [dbo].[EnrolmentStatus] AS Target
    USING (VALUES
    (N'5EAD4545-C006-1C1A-EB86-49AFC23F6EE1', N'Current (single registration at this school)', N'C', 0),
    (N'B77E95B0-8F93-06E8-A14B-050AFE215947', N'Current Main (dual registration)', N'M', 0),
    (N'832ABE83-BC93-715C-F534-528AB329C3E7', N'Current Subsidiary (dual registration)', N'S', 0),
    (N'B77F4F42-34EF-773F-228A-311AF49CC0CD', N'FE College', N'F', 0),
    (N'E597284A-2E12-AF45-22A7-F5D7BE60DCCC', N'Other Provider', N'O', 800),
    (N'216E9B0A-5AAE-7F57-AD5B-8FE17E49F855', N'Guest (pupil not registered at this school but attending some lessons or sessions)', N'G', 0),
    (N'A236B72E-D1B4-57EB-07E2-81FABEF8D544', N'Previous', N'P', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[EnrolmentStatus] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'5EAD4545-C006-1C1A-EB86-49AFC23F6EE1'),(N'B77E95B0-8F93-06E8-A14B-050AFE215947'),(N'832ABE83-BC93-715C-F534-528AB329C3E7'),(N'B77F4F42-34EF-773F-228A-311AF49CC0CD'),(N'E597284A-2E12-AF45-22A7-F5D7BE60DCCC'),(N'216E9B0A-5AAE-7F57-AD5B-8FE17E49F855'),(N'A236B72E-D1B4-57EB-07E2-81FABEF8D544')) v([Id]));
GO

-- BoarderStatus (4 rows) -----------------------------------------------
MERGE INTO [dbo].[BoarderStatus] AS Target
    USING (VALUES
    (N'007A3C94-3629-D1A2-9E41-FBDFA2D8DDB0', N'Boarder, nights per week not specified', N'B', 0),
    (N'76618725-E825-659A-532B-8D9D67DAC458', N'Boarder, six nights or less a week (special school only)', N'6', 0),
    (N'E585DEEB-F88F-25F2-A424-D1FE3627C22D', N'Boarder, seven nights a week (special school only)', N'7', 0),
    (N'CDE4960D-D186-5A5E-58DE-4100112034BC', N'Not a boarder', N'N', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[BoarderStatus] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'007A3C94-3629-D1A2-9E41-FBDFA2D8DDB0'),(N'76618725-E825-659A-532B-8D9D67DAC458'),(N'E585DEEB-F88F-25F2-A424-D1FE3627C22D'),(N'CDE4960D-D186-5A5E-58DE-4100112034BC')) v([Id]));
GO

-- SenTypes (14 rows) ----------------------------------------------------
MERGE INTO [dbo].[SenTypes] AS Target
    USING (VALUES
    (N'0666443D-1B3A-C50F-876D-57BC9780A973', N'Specific Learning Difficulty', N'SPLD', 0),
    (N'5C014442-1646-8C16-6A8C-7E71B42FF89B', N'Moderate Learning Difficulty', N'MLD', 0),
    (N'1E2A9493-335C-96E0-51A2-95E571A9F6F6', N'Severe Learning Difficulty', N'SLD', 0),
    (N'AECB3300-5A41-991F-268F-76A9F1E2E5AA', N'Profound & Multiple Learning Difficulty', N'PMLD', 0),
    (N'3307096C-CFDF-9D27-9D1F-A733EF7BE647', N'Social, Emotional and Mental Health', N'SEMH', 0),
    (N'8D106DA8-BAF3-2146-E5F8-98899461600B', N'Speech, Language and Communication Needs', N'SLCN', 0),
    (N'C64DCFC3-078B-D716-DEC8-F476FDC99272', N'Hearing Impairment', N'HI', 0),
    (N'AFE5A511-FF43-421C-C432-89EB39C19CB3', N'Vision Impairment', N'VI', 0),
    (N'7E3038CA-F2C2-D4A2-B1F2-D7DB0A33F554', N'Multi-Sensory Impairment', N'MSI', 0),
    (N'2F016361-E77C-9D87-174F-F9D0EF1C44A5', N'Physical Disability', N'PD', 0),
    (N'0728E132-3398-CC7E-DFF8-C2D22D5714FB', N'Autistic Spectrum Disorder', N'ASD', 0),
    (N'76164176-4C2C-E458-0BA8-B3F14714A956', N'Down Syndrome', N'DS', 0),
    (N'92EF1FE5-D4B8-0365-B183-89CDB569F8B1', N'Other Difficulty/Disability', N'OTH', 800),
    (N'3F4242A9-D1EB-F495-18A8-A026DDDD71BC', N'SEN support but no specialist assessment of type of need', N'NSA', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[SenTypes] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'0666443D-1B3A-C50F-876D-57BC9780A973'),(N'5C014442-1646-8C16-6A8C-7E71B42FF89B'),(N'1E2A9493-335C-96E0-51A2-95E571A9F6F6'),(N'AECB3300-5A41-991F-268F-76A9F1E2E5AA'),(N'3307096C-CFDF-9D27-9D1F-A733EF7BE647'),(N'8D106DA8-BAF3-2146-E5F8-98899461600B'),(N'C64DCFC3-078B-D716-DEC8-F476FDC99272'),(N'AFE5A511-FF43-421C-C432-89EB39C19CB3'),(N'7E3038CA-F2C2-D4A2-B1F2-D7DB0A33F554'),(N'2F016361-E77C-9D87-174F-F9D0EF1C44A5'),(N'0728E132-3398-CC7E-DFF8-C2D22D5714FB'),(N'76164176-4C2C-E458-0BA8-B3F14714A956'),(N'92EF1FE5-D4B8-0365-B183-89CDB569F8B1'),(N'3F4242A9-D1EB-F495-18A8-A026DDDD71BC')) v([Id]));
GO

-- ExclusionTypes (3 rows) ----------------------------------------------
MERGE INTO [dbo].[ExclusionTypes] AS Target
    USING (VALUES
    (N'C4AD61E6-3341-B4A5-3F73-3979697BFBB0', N'Permanent', N'PERM', 0, 1),
    (N'23A29EEF-8155-2EA1-5884-8C8CD2E6CC96', N'Lunchtime', N'LNCH', 0, 1),
    (N'8D6BDC9B-BDF4-95C5-DD81-6E5F5109EF7E', N'Suspension', N'SUSP', 0, 1)
    ) AS Source (Id, Description, Code, DisplayOrder, IsSystem)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, IsSystem, Active) VALUES (Id, Description, Code, DisplayOrder, IsSystem, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, IsSystem = Source.IsSystem, Active = 1;
GO
UPDATE [dbo].[ExclusionTypes] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'C4AD61E6-3341-B4A5-3F73-3979697BFBB0'),(N'23A29EEF-8155-2EA1-5884-8C8CD2E6CC96'),(N'8D6BDC9B-BDF4-95C5-DD81-6E5F5109EF7E')) v([Id]));
GO

-- ExclusionReasons (29 rows) --------------------------------------------
MERGE INTO [dbo].[ExclusionReasons] AS Target
    USING (VALUES
    (N'6D107981-DE1F-A542-C54D-F67B9DE8DE81', N'Physical assault against a pupil', N'PP', 0, 1),
    (N'2DECDC79-154B-2300-AF32-9A27090A65C2', N'Physical assault against an adult', N'PA', 0, 1),
    (N'ABCB32AF-611A-7E31-C8B5-F4288B88ED68', N'Verbal abuse/threatening behaviour against a pupil', N'VP', 0, 1),
    (N'7EBC53E6-74F5-5DE5-9522-C2568EFF5F60', N'Verbal abuse/threatening behaviour against an adult', N'VA', 0, 1),
    (N'232612C3-6A14-BA3B-9EE2-33EC94556BBE', N'Use or threat of use of an offensive weapon or prohibited item', N'OW', 0, 1),
    (N'9748C46C-5697-6E37-2A0A-8DC2628E0234', N'Found to have brought a weapon or prohibited item into school', N'BW', 0, 1),
    (N'B301FFF5-51B3-C230-E5B9-5F1745990A30', N'Bullying', N'BU', 0, 1),
    (N'F96C2249-789D-C860-EF13-8F4E6BBA69E7', N'Cyber-bullying or threatening behaviour online', N'TB', 0, 1),
    (N'7BB2FE75-3EFA-77CB-DC91-D93F6CFB1488', N'Racist abuse including bullying', N'RA', 0, 1),
    (N'872D8AED-273A-D451-8E1F-699D2F693265', N'Abuse against sexual orientation and gender identity including bullying', N'LG', 0, 1),
    (N'E3449E7A-3B8D-102B-37AA-E42AE9D32A94', N'Abuse relating to disability including bullying', N'DS', 0, 1),
    (N'AA05558C-B922-7671-EE6C-ED840B3529A0', N'Sexual misconduct', N'SM', 0, 1),
    (N'BA2847FB-7844-035C-7F2A-A25EBC68A627', N'Harmful sexual behaviour', N'SB', 0, 1),
    (N'1CB96A82-CCB8-8146-97B2-92ADB86E9565', N'Misogynistic behaviour', N'MB', 0, 1),
    (N'47807374-2F3B-2FAC-CFB9-902928622103', N'Misandry Behaviour', N'AB', 0, 1),
    (N'3B5AC305-B0A6-B168-2039-142C5F56A341', N'Drug related', N'DR', 0, 1),
    (N'84788074-AADD-B864-F6B8-386FB5DA96F5', N'Alcohol Related', N'AR', 0, 1),
    (N'1126B9D3-307C-C91E-B48F-0285F69C46D0', N'Smoking Related', N'SR', 0, 1),
    (N'6A7C3BB8-0A02-C073-D51A-50B870BDD83A', N'Vape Related', N'VR', 0, 1),
    (N'936B91E0-C5BA-FE36-71E9-56A95208843D', N'Drug and alcohol related', N'DA', 0, 1),
    (N'C8235387-428F-657D-A807-A1DF05F7AB06', N'Damage to property', N'DM', 0, 1),
    (N'C793C491-DD04-574D-D0C3-560EEC7A77CD', N'Theft', N'TH', 0, 1),
    (N'32FD7DC8-7C8F-0794-5A41-03E188663594', N'Persistent disruptive behaviour - low level disruption', N'LD', 0, 1),
    (N'C7119751-1281-7647-3085-C712A71A262F', N'Persistent disruptive behaviour – challenging behaviour', N'CB', 0, 1),
    (N'A2B65E91-60C7-CCF7-D775-ACEE76532499', N'Persistent or general disruptive behaviour', N'DB', 0, 1),
    (N'DADE34B4-F44F-0EDA-730C-820831E677FC', N'Inappropriate use of social media or online technology', N'MT', 0, 1),
    (N'45F7190B-71F2-E23E-165F-DE68E3B8022B', N'Use of Mobile Phone', N'MP', 0, 1),
    (N'F5337DEB-E2D9-7B1A-A60B-CA39B4D92425', N'Misbehaviour outside of school', N'OS', 0, 1),
    (N'9FC69FAD-786D-DCF2-FD38-E9C4001E6493', N'Wilful and repeated transgression of protective measures in place to protect public health', N'PH', 0, 1)
    ) AS Source (Id, Description, Code, DisplayOrder, IsSystem)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, IsSystem, Active) VALUES (Id, Description, Code, DisplayOrder, IsSystem, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, IsSystem = Source.IsSystem, Active = 1;
GO
UPDATE [dbo].[ExclusionReasons] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'6D107981-DE1F-A542-C54D-F67B9DE8DE81'),(N'2DECDC79-154B-2300-AF32-9A27090A65C2'),(N'ABCB32AF-611A-7E31-C8B5-F4288B88ED68'),(N'7EBC53E6-74F5-5DE5-9522-C2568EFF5F60'),(N'232612C3-6A14-BA3B-9EE2-33EC94556BBE'),(N'9748C46C-5697-6E37-2A0A-8DC2628E0234'),(N'B301FFF5-51B3-C230-E5B9-5F1745990A30'),(N'F96C2249-789D-C860-EF13-8F4E6BBA69E7'),(N'7BB2FE75-3EFA-77CB-DC91-D93F6CFB1488'),(N'872D8AED-273A-D451-8E1F-699D2F693265'),(N'E3449E7A-3B8D-102B-37AA-E42AE9D32A94'),(N'AA05558C-B922-7671-EE6C-ED840B3529A0'),(N'BA2847FB-7844-035C-7F2A-A25EBC68A627'),(N'1CB96A82-CCB8-8146-97B2-92ADB86E9565'),(N'47807374-2F3B-2FAC-CFB9-902928622103'),(N'3B5AC305-B0A6-B168-2039-142C5F56A341'),(N'84788074-AADD-B864-F6B8-386FB5DA96F5'),(N'1126B9D3-307C-C91E-B48F-0285F69C46D0'),(N'6A7C3BB8-0A02-C073-D51A-50B870BDD83A'),(N'936B91E0-C5BA-FE36-71E9-56A95208843D'),(N'C8235387-428F-657D-A807-A1DF05F7AB06'),(N'C793C491-DD04-574D-D0C3-560EEC7A77CD'),(N'32FD7DC8-7C8F-0794-5A41-03E188663594'),(N'C7119751-1281-7647-3085-C712A71A262F'),(N'A2B65E91-60C7-CCF7-D775-ACEE76532499'),(N'DADE34B4-F44F-0EDA-730C-820831E677FC'),(N'45F7190B-71F2-E23E-165F-DE68E3B8022B'),(N'F5337DEB-E2D9-7B1A-A60B-CA39B4D92425'),(N'9FC69FAD-786D-DCF2-FD38-E9C4001E6493')) v([Id]));
GO
