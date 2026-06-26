-- ============================================================================
-- Seed the v1 CBDS lookups added in 20260626000600. MERGE upsert (Active = 1) +
-- deactivate-non-CBDS. Ids deterministic = md5(table + '|' + code). Plus a patch
-- adding the missing EHC-plan stage (E) to SenStatus.
-- ============================================================================

-- ExclusionReviewCategories (CS137, 2 rows) ---------------------
MERGE INTO [dbo].[ExclusionReviewCategories] AS Target
    USING (VALUES
    (N'31A9D8D7-41A2-8403-CE77-6B76492B9554', N'Governing Body', N'GB', 0),
    (N'E2E1D765-51CC-D8CE-0392-AE8A496668A8', N'Independent Review Panel', N'IRP', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[ExclusionReviewCategories] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'31A9D8D7-41A2-8403-CE77-6B76492B9554'),(N'E2E1D765-51CC-D8CE-0392-AE8A496668A8')) v([Id]));
GO

-- SpecialSchoolOrganisations (CS038, 3 rows) --------------------
MERGE INTO [dbo].[SpecialSchoolOrganisations] AS Target
    USING (VALUES
    (N'0435205C-4A39-05A7-8C59-9F888EDCF6DF', N'Day pupils (mainly)', N'D', 0),
    (N'09FF13C7-3E51-05E2-A426-DDFEEA68F3E6', N'Boarding pupils (mainly)', N'B', 0),
    (N'E8B7560C-3B0E-4031-FFC3-74B5118CDBE7', N'Hospital Special School', N'H', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[SpecialSchoolOrganisations] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'0435205C-4A39-05A7-8C59-9F888EDCF6DF'),(N'09FF13C7-3E51-05E2-A426-DDFEEA68F3E6'),(N'E8B7560C-3B0E-4031-FFC3-74B5118CDBE7')) v([Id]));
GO

-- SpecialSchoolTypes (CS077, 12 rows) ----------------------------
MERGE INTO [dbo].[SpecialSchoolTypes] AS Target
    USING (VALUES
    (N'CC397A6F-E498-918E-B4A6-43B7DEED2751', N'Specific Learning Difficulty', N'SPLD', 0),
    (N'C5217AC9-F1C4-644A-B726-D4FA290BE9E7', N'Moderate Learning Difficulty', N'MLD', 0),
    (N'321F1D56-28F7-ACA2-6653-3B0F2D1CA0B6', N'Severe Learning Difficulty', N'SLD', 0),
    (N'E4E21DAB-7A08-3E5F-7842-C21CD797EE8B', N'Profound & Multiple Learning Difficulty', N'PMLD', 0),
    (N'2A30F96E-5B53-490C-61AB-E0AD126DC2CC', N'Social, Emotional and Mental Health', N'SEMH', 0),
    (N'9E1D4A04-1094-23EF-84D8-73BDB9E5D260', N'Speech, Language and Communication Needs', N'SLCN', 0),
    (N'AA87D1A8-8479-4AEC-91A2-3864FA5BD464', N'Hearing Impairment', N'HI', 0),
    (N'D8F2ACF8-157F-E3EE-AA3F-608DA2B8D748', N'Visual Impairment', N'VI', 0),
    (N'11D824F0-E141-3DF0-353C-ED407CA516A0', N'Multi-Sensory Impairment', N'MSI', 0),
    (N'17253B6A-308A-C901-3E46-5FE5CE5B5363', N'Physical Disability', N'PD', 0),
    (N'7CD59054-53E1-A526-A3E2-C866A29AB999', N'Autistic Spectrum Disorder', N'ASD', 0),
    (N'E30A1A3A-0789-347C-BC7F-90F76284928A', N'Other Difficulty/Disability', N'OTH', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[SpecialSchoolTypes] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'CC397A6F-E498-918E-B4A6-43B7DEED2751'),(N'C5217AC9-F1C4-644A-B726-D4FA290BE9E7'),(N'321F1D56-28F7-ACA2-6653-3B0F2D1CA0B6'),(N'E4E21DAB-7A08-3E5F-7842-C21CD797EE8B'),(N'2A30F96E-5B53-490C-61AB-E0AD126DC2CC'),(N'9E1D4A04-1094-23EF-84D8-73BDB9E5D260'),(N'AA87D1A8-8479-4AEC-91A2-3864FA5BD464'),(N'D8F2ACF8-157F-E3EE-AA3F-608DA2B8D748'),(N'11D824F0-E141-3DF0-353C-ED407CA516A0'),(N'17253B6A-308A-C901-3E46-5FE5CE5B5363'),(N'7CD59054-53E1-A526-A3E2-C866A29AB999'),(N'E30A1A3A-0789-347C-BC7F-90F76284928A')) v([Id]));
GO

-- PupilPremiumIndicators (CS106, 5 rows) ------------------------
MERGE INTO [dbo].[PupilPremiumIndicators] AS Target
    USING (VALUES
    (N'F19EF366-5972-CEA3-C21F-3D87DE344DFB', N'Deprivation Pupil Premium', N'DPP', 0),
    (N'EF9A660A-D27E-D221-4DFB-E2083541F617', N'Service Child Premium', N'SCP', 0),
    (N'B0F981D0-E91B-F47C-84CD-E40455B41512', N'Looked After Premium', N'LAP', 0),
    (N'F7A2632E-5F1C-E013-4433-5D5875289991', N'Adopted from Care Premium', N'AFC', 0),
    (N'CE4C636E-A4E0-8175-9D1E-2D793F187C4B', N'Early Years Pupil Premium', N'EYP', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[PupilPremiumIndicators] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'F19EF366-5972-CEA3-C21F-3D87DE344DFB'),(N'EF9A660A-D27E-D221-4DFB-E2083541F617'),(N'B0F981D0-E91B-F47C-84CD-E40455B41512'),(N'F7A2632E-5F1C-E013-4433-5D5875289991'),(N'CE4C636E-A4E0-8175-9D1E-2D793F187C4B')) v([Id]));
GO

-- FsmCategories (CS133, 3 rows) ---------------------------------
MERGE INTO [dbo].[FsmCategories] AS Target
    USING (VALUES
    (N'055952A2-4359-4BAB-69B9-539DF0070499', N'Targeted FSM', N'TFSM', 0),
    (N'F5F65A75-751C-0E82-BFD6-028BEFA3C0F1', N'Expanded FSM', N'EFSM', 0),
    (N'9D7E7E50-C14B-E2EA-E341-636D7C80AD3F', N'Other FSM', N'OFSM', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[FsmCategories] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'055952A2-4359-4BAB-69B9-539DF0070499'),(N'F5F65A75-751C-0E82-BFD6-028BEFA3C0F1'),(N'9D7E7E50-C14B-E2EA-E341-636D7C80AD3F')) v([Id]));
GO

-- ClassOfDegrees (CS062, 16 rows) --------------------------------
MERGE INTO [dbo].[ClassOfDegrees] AS Target
    USING (VALUES
    (N'11108524-AA71-804D-6EDD-CCEBDDBF6772', N'First class honours', N'1', 0),
    (N'E745CBA2-11D3-61BA-F1F0-C96E1325AC8F', N'Upper second class honours', N'2', 0),
    (N'39F91723-F8B7-87AF-90D5-EB6E87066C31', N'Lower second class honours', N'3', 0),
    (N'D1DEA3D1-7276-FFAC-037A-514303BAF3F5', N'Undivided second class honours', N'4', 0),
    (N'70E8374B-86EF-9271-15FD-12601696EA0C', N'Third class honours', N'5', 0),
    (N'17F2DB8D-5EA6-8F22-4375-539CE2DEAC93', N'Fourth class honours', N'6', 0),
    (N'8FBD1A96-58E6-582E-318A-91CF302BEC99', N'Unclassified honours', N'7', 0),
    (N'57D1FFE1-8729-2461-95BB-F315E5584A84', N'Aegrotat (whether to honours or pass)**', N'8', 0),
    (N'027AC83E-9076-89C6-0941-DDC73951A1E4', N'Pass - degree awarded without honours following an honours degree course', N'9', 0),
    (N'A892B26D-6D89-CCB6-8656-961239A08CD4', N'Ordinary (to include divisions of ordinary, if any) - degree awarded following a non-honours course', N'10', 0),
    (N'9D642C1F-BB41-97B9-FA45-1636A261F738', N'General degree - degree awarded after following a non-honours course/degree that was not available to be classified', N'11', 0),
    (N'9B719E25-8180-4D64-3D90-1F561A79300F', N'Degree awarded outside the UK and Eire', N'12', 0),
    (N'6B246C8A-A901-D593-E19E-40092245FB82', N'Masters Degree**', N'13', 0),
    (N'C9154238-049A-065E-E6F9-0865B1BCE859', N'Doctorate**', N'14', 0),
    (N'85161A47-2105-74D1-7CE2-2A06EC4BBC73', N'Ordinary Or Pass - applicable for a non-degree course**', N'15', 0),
    (N'684DE5F5-2D6A-BCD3-93C3-ABE9CD124757', N'Not known', N'99', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[ClassOfDegrees] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'11108524-AA71-804D-6EDD-CCEBDDBF6772'),(N'E745CBA2-11D3-61BA-F1F0-C96E1325AC8F'),(N'39F91723-F8B7-87AF-90D5-EB6E87066C31'),(N'D1DEA3D1-7276-FFAC-037A-514303BAF3F5'),(N'70E8374B-86EF-9271-15FD-12601696EA0C'),(N'17F2DB8D-5EA6-8F22-4375-539CE2DEAC93'),(N'8FBD1A96-58E6-582E-318A-91CF302BEC99'),(N'57D1FFE1-8729-2461-95BB-F315E5584A84'),(N'027AC83E-9076-89C6-0941-DDC73951A1E4'),(N'A892B26D-6D89-CCB6-8656-961239A08CD4'),(N'9D642C1F-BB41-97B9-FA45-1636A261F738'),(N'9B719E25-8180-4D64-3D90-1F561A79300F'),(N'6B246C8A-A901-D593-E19E-40092245FB82'),(N'C9154238-049A-065E-E6F9-0865B1BCE859'),(N'85161A47-2105-74D1-7CE2-2A06EC4BBC73'),(N'684DE5F5-2D6A-BCD3-93C3-ABE9CD124757')) v([Id]));
GO

-- ExclusionAppealResults (CS105, 14 rows) ------------------------
MERGE INTO [dbo].[ExclusionAppealResults] AS Target
    USING (VALUES
    (N'F1DEA694-955A-F630-8A1F-4F1787112D1A', N'After initial consideration governing board decided that pupil should not be reinstated and no request for an Independent Review Panel was made (pupil not offered reinstatement)', N'ANO', 0, 1),
    (N'13D40E84-87EB-7BD8-36FF-A979D1E1A7AC', N'The governing board has decided that the pupil should be reinstated, and the relevant person has not declined reinstatement, but the pupil has not yet resumed attendance at the school; (pupil offered reinstatement)', N'BRE', 0, 1),
    (N'18861B86-6612-9AFA-2B6C-5ADC3176FE2D', N'The governing board has decided the pupil should be reinstated but the relevant person has declined reinstatement (pupil is not offered reinstatement);', N'CNO', 0, 1),
    (N'3754D999-862C-AB10-BEA9-DB344217928A', N'The governing board has decided that the pupil should not be reinstated, the time for an application for a review has not yet expired, and the relevant person has not yet given notice in writing that they do not intend to apply for a review; (pupil not offered reinstatement)', N'DNO', 0, 1),
    (N'178C45BF-2EAA-F776-8722-181CE11046F9', N'Parents made representations where the governing board is required to consider reinstatement, (review ongoing)', N'EON', 0, 1),
    (N'E63649BA-F927-58D4-E564-2F8A6349956A', N'Pupil offered reinstatement after initial governing board (pupil offered reinstatement)', N'FRE', 0, 1),
    (N'A28089EB-36CA-7809-69ED-CDE69159ECDE', N'Pupil not offered reinstatement after initial governing board, a request for an Independent Review Panel was made but not determined (pupil not offered reinstatement)', N'GNO', 0, 1),
    (N'7E4F7931-A4DA-4540-6A83-EC8359A7B039', N'Withdrawn', N'HWD', 0, 1),
    (N'E7A80A89-3C91-43CC-AC15-A11F797E7BC0', N'Independent review panel upheld governing board’s initial decision that pupil should not be reinstated (pupil not offered reinstatement)', N'INO', 0, 1),
    (N'900E3AE8-F24F-7947-F54C-9147040929C0', N'Pupil offered reinstatement after independent review panel recommended governing board to reconsider reinstatement (pupil offered reinstatement)', N'JRE', 0, 1),
    (N'30DC1AA3-6FF7-F6BB-119C-1A773834E36D', N'Pupil offered reinstatement after independent review panel directed governing board to reconsider reinstatement (pupil offered reinstatement)', N'KRE', 0, 1),
    (N'1A3101DD-6B06-1600-BE1C-1F864D61BEB2', N'Pupil not offered reinstatement after independent review panel recommended governing board to reconsider reinstatement (pupil not offered reinstatement)', N'LNO', 0, 1),
    (N'AE3A8B5A-8CA6-32F5-6B13-37ECF5816891', N'Pupil not offered reinstatement after independent review panel directed governing board to reconsider reinstatement (pupil not offered reinstatement)', N'MNO', 0, 1),
    (N'5E07A871-6E39-3ABB-AD8D-A59EF3C4A7CF', N'Ongoing (no decision yet)', N'NOG', 0, 1)
    ) AS Source (Id, Description, Code, DisplayOrder, IsSystem)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, IsSystem, Active) VALUES (Id, Description, Code, DisplayOrder, IsSystem, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, IsSystem = Source.IsSystem, Active = 1;
GO
UPDATE [dbo].[ExclusionAppealResults] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'F1DEA694-955A-F630-8A1F-4F1787112D1A'),(N'13D40E84-87EB-7BD8-36FF-A979D1E1A7AC'),(N'18861B86-6612-9AFA-2B6C-5ADC3176FE2D'),(N'3754D999-862C-AB10-BEA9-DB344217928A'),(N'178C45BF-2EAA-F776-8722-181CE11046F9'),(N'E63649BA-F927-58D4-E564-2F8A6349956A'),(N'A28089EB-36CA-7809-69ED-CDE69159ECDE'),(N'7E4F7931-A4DA-4540-6A83-EC8359A7B039'),(N'E7A80A89-3C91-43CC-AC15-A11F797E7BC0'),(N'900E3AE8-F24F-7947-F54C-9147040929C0'),(N'30DC1AA3-6FF7-F6BB-119C-1A773834E36D'),(N'1A3101DD-6B06-1600-BE1C-1F864D61BEB2'),(N'AE3A8B5A-8CA6-32F5-6B13-37ECF5816891'),(N'5E07A871-6E39-3ABB-AD8D-A59EF3C4A7CF')) v([Id]));
GO

-- SenStatus: add the EHC-plan stage 'E' (CBDS CS096), missing from the original
-- seed (which predates the 2014 SEND reforms). Additive — other stages untouched.
MERGE INTO [dbo].[SenStatus] AS Target
    USING (VALUES (N'71D39DF5-B8CA-4EAA-AD51-5B07C2304F27', N'E', N'Education, Health and Care Plan')) AS Source (Id, Code, Description)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Code, Description, Active) VALUES (Id, Code, Description, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Active = 1;
GO
