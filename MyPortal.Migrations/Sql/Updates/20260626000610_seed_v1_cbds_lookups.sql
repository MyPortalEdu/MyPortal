-- ============================================================================
-- Seed the v1 CBDS lookups added in 20260626000600. MERGE upsert (Active = 1) +
-- deactivate-non-CBDS. Ids deterministic = md5(table + '|' + code). Plus a patch
-- adding the missing EHC-plan stage (E) to SenStatus.
-- ============================================================================

-- ExamComponentResultTypes (CS094, 33 rows) ----------------------
MERGE INTO [dbo].[ExamComponentResultTypes] AS Target
    USING (VALUES
    (N'C305FE7B-7DCD-1B05-3E83-5A7D8E92062A', N'Examination/Post-14 Qualification Grade', N'EG', 0),
    (N'BB187823-3896-778D-4326-012043AA8C30', N'EAL Assessment Level (Pre NC Level 2)', N'EL', 0),
    (N'9E066A03-7882-FB86-0D5B-9B946DCE32FA', N'Foundation Stage Profile Count', N'FC', 0),
    (N'856A4C43-C652-B351-9C4F-D25F04600386', N'Foundation Stage Profile Detail', N'FD', 0),
    (N'FCDEE355-B68B-0429-EEC7-04892F7E8D9B', N'Foundation Stage Profile Score', N'FS', 0),
    (N'98667642-D982-828D-1A0F-CD51374DFBBF', N'International Baccalaureate Assessment', N'IA', 0),
    (N'AD43BF64-2EEC-95EA-98D1-3EDD398BEF59', N'International Baccalaureate Grade Point', N'IG', 0),
    (N'7C7D10FE-8A8E-BE3C-A3CF-A558CF63CB3E', N'International Baccalaureate Final Result', N'IR', 0),
    (N'44F84C77-B51E-8DEB-CA87-6C9169BF1853', N'International Baccalaureate Aggregate Grade Points', N'IS', 0),
    (N'34D7BB4B-FE96-DF71-CEB2-BEE0008D7137', N'Assessment for Learning Level', N'LL', 0),
    (N'2A742B17-D95E-9960-6CA3-E22DA86BBBCC', N'National Curriculum Age Standardised Score', N'NA', 0),
    (N'4577536E-81CF-71EA-CD77-F864B905BBDE', N'National Curriculum Scaled Score', N'NB', 0),
    (N'6D794F35-5E48-89D9-8616-628A445B50A9', N'National Curriculum Performance Descriptors', N'NC', 0),
    (N'DE917940-B254-04E0-4A4B-87F48653192B', N'National Curriculum Level (Decimalised)', N'ND', 0),
    (N'1D046F48-E608-7CF4-DE4A-CA8E71EEA7FB', N'Welsh National Test Progress Score Difference', N'NE', 0),
    (N'F5CA4BC3-5B60-F941-09D3-A64D529B5BFA', N'National Curriculum Level with Fine Grading', N'NF', 0),
    (N'B7B5533C-515D-B57B-6370-18977D64A7FF', N'Welsh National Test Progress Score', N'NG', 0),
    (N'F3246931-67B4-99DF-8DBA-ED60FA2056FC', N'National Curriculum Level', N'NL', 0),
    (N'B49946BA-15E5-FCF8-EFF4-BDF53537BFE1', N'National Curriculum Task/Test Mark', N'NM', 0),
    (N'BB6A88BD-E700-7612-D212-1ACBFBBA6979', N'National Curriculum Level (Numeric Equivalent)', N'NN', 0),
    (N'B61B3CA3-ED5A-1D70-89FB-AAD8E11D847D', N'National Curriculum Level (Order)', N'NO', 0),
    (N'5CC71A1C-D070-FC24-6C9E-4D783E526912', N'SEN Assessment Level (P Scale)', N'NP', 0),
    (N'4226799F-2591-D039-40E3-0A6FC93BECEB', N'National Curriculum Level (QCA Points Score)', N'NQ', 0),
    (N'E7F99B9E-D2C2-4289-9988-8D6293EE7844', N'National Curriculum Test Raw Score', N'NR', 0),
    (N'653828B3-6978-00D5-CDAB-EE6E1982D3B8', N'National Curriculum Summary (Aggregate) Mark', N'NS', 0),
    (N'EEEA81EB-0018-39F8-6E2A-23C24E6614E0', N'Assessment, free text to summarise school judgements', N'NT', 0),
    (N'32CC495D-0B50-4CC3-18A3-82CA08DC9C7F', N'Phonics Screening Check Outcome', N'NY', 0),
    (N'CBBF84BC-4489-C034-5502-F01A40862854', N'CEM Reception Baseline Age-corrected score', N'RA', 0),
    (N'F88DBC45-B210-87DA-6E60-07D2C741A11C', N'CEM Reception Baseline Score', N'RC', 0),
    (N'70A2B69A-2FF9-3CF7-CF36-84FF640EEEE8', N'Early Excellence Reception Baseline Score', N'RE', 0),
    (N'684EF837-588E-1286-9692-EF80157496D1', N'NFER Reception Baseline Score', N'RN', 0),
    (N'1C869DA6-019E-8C27-0347-83A4BAF7F63A', N'NFER Reception Baseline Age-Adjusted Score', N'RS', 0),
    (N'D8C40D1C-4153-00DB-58E3-AF4765F7E81F', N'Early Excellence Reception Baseline Results, text', N'RX', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[ExamComponentResultTypes] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'C305FE7B-7DCD-1B05-3E83-5A7D8E92062A'),(N'BB187823-3896-778D-4326-012043AA8C30'),(N'9E066A03-7882-FB86-0D5B-9B946DCE32FA'),(N'856A4C43-C652-B351-9C4F-D25F04600386'),(N'FCDEE355-B68B-0429-EEC7-04892F7E8D9B'),(N'98667642-D982-828D-1A0F-CD51374DFBBF'),(N'AD43BF64-2EEC-95EA-98D1-3EDD398BEF59'),(N'7C7D10FE-8A8E-BE3C-A3CF-A558CF63CB3E'),(N'44F84C77-B51E-8DEB-CA87-6C9169BF1853'),(N'34D7BB4B-FE96-DF71-CEB2-BEE0008D7137'),(N'2A742B17-D95E-9960-6CA3-E22DA86BBBCC'),(N'4577536E-81CF-71EA-CD77-F864B905BBDE'),(N'6D794F35-5E48-89D9-8616-628A445B50A9'),(N'DE917940-B254-04E0-4A4B-87F48653192B'),(N'1D046F48-E608-7CF4-DE4A-CA8E71EEA7FB'),(N'F5CA4BC3-5B60-F941-09D3-A64D529B5BFA'),(N'B7B5533C-515D-B57B-6370-18977D64A7FF'),(N'F3246931-67B4-99DF-8DBA-ED60FA2056FC'),(N'B49946BA-15E5-FCF8-EFF4-BDF53537BFE1'),(N'BB6A88BD-E700-7612-D212-1ACBFBBA6979'),(N'B61B3CA3-ED5A-1D70-89FB-AAD8E11D847D'),(N'5CC71A1C-D070-FC24-6C9E-4D783E526912'),(N'4226799F-2591-D039-40E3-0A6FC93BECEB'),(N'E7F99B9E-D2C2-4289-9988-8D6293EE7844'),(N'653828B3-6978-00D5-CDAB-EE6E1982D3B8'),(N'EEEA81EB-0018-39F8-6E2A-23C24E6614E0'),(N'32CC495D-0B50-4CC3-18A3-82CA08DC9C7F'),(N'CBBF84BC-4489-C034-5502-F01A40862854'),(N'F88DBC45-B210-87DA-6E60-07D2C741A11C'),(N'70A2B69A-2FF9-3CF7-CF36-84FF640EEEE8'),(N'684EF837-588E-1286-9692-EF80157496D1'),(N'1C869DA6-019E-8C27-0347-83A4BAF7F63A'),(N'D8C40D1C-4153-00DB-58E3-AF4765F7E81F')) v([Id]));
GO

-- AssessmentStages (CS064, 36 rows) ------------------------------
MERGE INTO [dbo].[AssessmentStages] AS Target
    USING (VALUES
    (N'79EA1B68-3845-9A56-7709-6731592B9D72', N'Assessment for Learning', N'AFL', 0),
    (N'9FB5E874-07F7-0BDA-04DF-0FEBC9773388', N'Early Years Foundation Stage+***', N'EYF', 0),
    (N'B9CC9F56-50F9-2644-0C69-52AC3E8F66DB', N'Foundation Stage +***', N'FSP', 0),
    (N'A36F8EFF-A50E-BB9E-A04F-8A90D0DDEB82', N'Baseline Assessment', N'KS0', 0),
    (N'210A72AB-E0D6-572E-0BDB-5625C1CB7D6D', N'End of Key Stage 1 +', N'KS1', 0),
    (N'B2130E2B-7D0D-92DA-1915-0DC65B954F90', N'Key Stage 1 Trial (Historical) +', N'K1T', 0),
    (N'B5BA5E14-ED5B-0B40-F5C9-6D8C2543106B', N'Key Stage 2 Progression Teacher Assessments (Pilot) +', N'K2P', 0),
    (N'E451E59D-9661-5497-33A1-4E2107DFF255', N'Key Stage 3 Progression Teacher Assessments (Pilot) +', N'K3P', 0),
    (N'FC14449E-404B-5B0F-1766-5E16359AD5EB', N'End of Key Stage 2 +', N'KS2', 0),
    (N'42CDC9D6-2CA6-6E0C-007A-E20C75AA6355', N'End of Key Stage 3 +', N'KS3', 0),
    (N'E6394450-BBD3-FF7B-9B9C-3C7FF748C261', N'GCSE/GNVQ/Other Approved Awards', N'KS4', 900),
    (N'45F2AAA3-2244-2BE8-35F6-43FD17406A25', N'A Level/AS Level/Advanced GNVQ/Other Advanced Studies', N'KS5', 900),
    (N'C743B776-B5CA-2FF6-D327-11FD1D6C8575', N'Year 3 Optional Tests/Teacher Assessments', N'Y03', 0),
    (N'A4AEDB13-C574-8850-3723-9223F17D7870', N'Year 4 Optional Tests/Teacher Assessments', N'Y04', 0),
    (N'253F5234-C5A5-EECC-F673-17017A722A2D', N'Year 4 Optional Tests (1997 Based)', N'Y4X', 0),
    (N'2150388C-BA5F-DE1C-F63A-15337EE622AD', N'Year 5 Optional Tests/Teacher Assessments', N'Y05', 0),
    (N'3A26E29B-DBE0-5421-55AE-64E8009DD230', N'Year 7 Optional Tests/Teacher Assessments', N'Y07', 0),
    (N'ECEF9934-516B-2C6D-AD85-CD0C6DB44D10', N'Year 7 Progress Tests (Historical) **', N'Y7P', 0),
    (N'192D6172-4BFF-D459-0218-F126B879FD2D', N'Year 8 Optional Tests/Teacher Assessments', N'Y08', 0),
    (N'DBADD03D-CB61-3E61-6FF9-44F5D0E840F6', N'English as an Additional Language Level of Acquisition', N'EAL', 0),
    (N'197F5DF1-0A0F-7E90-7EEF-EF254B151D88', N'P-Scale Assessment for SEN children', N'SEN', 0),
    (N'15C61ABB-279F-3425-9506-18C7F2DA7DD7', N'Single Level Test (Level 2) +', N'SL2', 0),
    (N'403BCD98-8F2E-6AE0-A10A-02A705A02D5C', N'Single Level Test (Level 3) +', N'SL3', 0),
    (N'55E52A08-EF85-625F-683F-5B3EC6C581B3', N'Single Level Test (Level 4) +', N'SL4', 0),
    (N'AC1625BD-F578-4B51-28B5-195AC9E67B38', N'Single Level Test (Level 5) +', N'SL5', 0),
    (N'F3A4CA4E-4993-4695-CCE1-237F87879C4F', N'Single Level Test (Level 6) +', N'SL6', 0),
    (N'5B2FF8C5-9BBB-34CD-424E-B3FC4ABD7FDE', N'World Class Tests – Aged 9 *', N'W09', 0),
    (N'D819603A-3D1E-4CFC-6810-BA89D81CBFA2', N'World Class Tests – Aged 13 *', N'W13', 0),
    (N'2F9CDC0C-8DA2-CD3D-880D-36FEAAF6CCE2', N'Welsh National Curriculum Year 2', N'NC2', 0),
    (N'BBF2E279-46A2-E77E-DAC1-8DEDBBF9AE4F', N'Welsh National Curriculum Year 3', N'NC3', 0),
    (N'1A7C4009-96BF-31C2-6E01-437A342F1848', N'Welsh National Curriculum Year 4', N'NC4', 0),
    (N'A811BEEA-3D76-D092-D0D0-AF0FB53FB9B1', N'Welsh National Curriculum Year 5', N'NC5', 0),
    (N'EC65C848-972B-3ECC-2524-9550FF64B5C1', N'Welsh National Curriculum Year 6', N'NC6', 0),
    (N'19AD7BC3-1D06-89C8-67CB-9B8D4FC90026', N'Welsh National Curriculum Year 7', N'NC7', 0),
    (N'5BA015CB-8D9D-D032-46F8-9C85EDE9B95E', N'Welsh National Curriculum Year 8', N'NC8', 0),
    (N'F20DC334-7198-DC2D-5BB7-A81C3198F9CA', N'Welsh National Curriculum Year 9', N'NC9', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[AssessmentStages] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'79EA1B68-3845-9A56-7709-6731592B9D72'),(N'9FB5E874-07F7-0BDA-04DF-0FEBC9773388'),(N'B9CC9F56-50F9-2644-0C69-52AC3E8F66DB'),(N'A36F8EFF-A50E-BB9E-A04F-8A90D0DDEB82'),(N'210A72AB-E0D6-572E-0BDB-5625C1CB7D6D'),(N'B2130E2B-7D0D-92DA-1915-0DC65B954F90'),(N'B5BA5E14-ED5B-0B40-F5C9-6D8C2543106B'),(N'E451E59D-9661-5497-33A1-4E2107DFF255'),(N'FC14449E-404B-5B0F-1766-5E16359AD5EB'),(N'42CDC9D6-2CA6-6E0C-007A-E20C75AA6355'),(N'E6394450-BBD3-FF7B-9B9C-3C7FF748C261'),(N'45F2AAA3-2244-2BE8-35F6-43FD17406A25'),(N'C743B776-B5CA-2FF6-D327-11FD1D6C8575'),(N'A4AEDB13-C574-8850-3723-9223F17D7870'),(N'253F5234-C5A5-EECC-F673-17017A722A2D'),(N'2150388C-BA5F-DE1C-F63A-15337EE622AD'),(N'3A26E29B-DBE0-5421-55AE-64E8009DD230'),(N'ECEF9934-516B-2C6D-AD85-CD0C6DB44D10'),(N'192D6172-4BFF-D459-0218-F126B879FD2D'),(N'DBADD03D-CB61-3E61-6FF9-44F5D0E840F6'),(N'197F5DF1-0A0F-7E90-7EEF-EF254B151D88'),(N'15C61ABB-279F-3425-9506-18C7F2DA7DD7'),(N'403BCD98-8F2E-6AE0-A10A-02A705A02D5C'),(N'55E52A08-EF85-625F-683F-5B3EC6C581B3'),(N'AC1625BD-F578-4B51-28B5-195AC9E67B38'),(N'F3A4CA4E-4993-4695-CCE1-237F87879C4F'),(N'5B2FF8C5-9BBB-34CD-424E-B3FC4ABD7FDE'),(N'D819603A-3D1E-4CFC-6810-BA89D81CBFA2'),(N'2F9CDC0C-8DA2-CD3D-880D-36FEAAF6CCE2'),(N'BBF2E279-46A2-E77E-DAC1-8DEDBBF9AE4F'),(N'1A7C4009-96BF-31C2-6E01-437A342F1848'),(N'A811BEEA-3D76-D092-D0D0-AF0FB53FB9B1'),(N'EC65C848-972B-3ECC-2524-9550FF64B5C1'),(N'19AD7BC3-1D06-89C8-67CB-9B8D4FC90026'),(N'5BA015CB-8D9D-D032-46F8-9C85EDE9B95E'),(N'F20DC334-7198-DC2D-5BB7-A81C3198F9CA')) v([Id]));
GO

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
