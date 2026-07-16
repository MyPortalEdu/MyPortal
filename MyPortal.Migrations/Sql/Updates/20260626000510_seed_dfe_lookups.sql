-- ============================================================================
-- Seed the CBDS-backed lookups added in 20260626000400 / 20260626000500 (data-
-- item gap analysis). MERGE upsert (Active = 1) + deactivate-non-CBDS, same as
-- the other CBDS seeds. Ids deterministic = md5(table + '|' + code).
-- ============================================================================

-- StaffOrigins (CS055, 21 rows) --------------------------------------
MERGE INTO [dbo].[StaffOrigins] AS Target
    USING (VALUES
    (N'0C7BE67E-BF54-D2F5-3497-1F42A9837207', N'First employment in teaching - immediately after training', N'1STIMM', 0),
    (N'07AAD2D2-1A6E-DD4B-CBAA-E0F96749178A', N'First employment in teaching - not immediately after training', N'1STAFT', 0),
    (N'51CB8C93-022B-A7B3-AF3E-75F4CDE8FEBB', N'First employment in teaching - employment based teacher training', N'1STEBR', 0),
    (N'908D7B4F-B26D-1848-08A2-5836AE6CCD56', N'Teaching post within the LA sector (school or central staff) in England or Wales', N'TCHLEA', 0),
    (N'2D4EFB06-CDCC-1D52-F377-5FE0637E9350', N'Teaching post within a Sixth form college in England or Wales', N'TCH6TH', 0),
    (N'B5CAA084-6C1C-4BAB-30B9-B7048EAB2FF2', N'Teaching post within an independent school in England or Wales', N'TCHIND', 0),
    (N'D63DE485-309D-D42B-56AA-DC58A06AF3F1', N'Teaching post within a University, FE/HE college in England or Wales', N'TCHFHE', 0),
    (N'DC683D03-B680-7214-5C25-6CBE64327C5A', N'Other education post in England or Wales', N'OTHEDU', 800),
    (N'7B32B2E7-8A5F-C16C-3955-0AC443704842', N'Teaching post in Scotland or Northern Ireland', N'TCHSNI', 0),
    (N'C23CB96F-6120-40B1-0CE8-735951FD4C44', N'Other education post in Scotland or Northern Ireland', N'OTHSNI', 800),
    (N'74C7CB11-FB61-1B0F-9C3C-D7C641BE80C1', N'Teaching post outside the UK', N'TCHFOR', 0),
    (N'C867FB2B-3BBF-F39F-6AD9-BDC71F4CC5BE', N'Other education post outside the UK', N'OTHFOR', 800),
    (N'51B6C57C-A299-6E20-0F3B-AA31D4C40462', N'Non-education employment - public sector', N'EMPPUB', 0),
    (N'96B5C9B8-9A98-EA07-B667-D97CC2E69B5D', N'Non-education employment - self-employment', N'EMPSLF', 0),
    (N'3FEBBCBC-691B-72FA-1F1F-BEE3E587E5E9', N'Non-education employment - other employment', N'EMPOTH', 800),
    (N'5706790A-309B-BEBF-E5D1-9772175BBDF3', N'Unemployed and seeking work', N'UNEMPL', 0),
    (N'F8D043F0-144B-019E-93A3-B99C9D643067', N'Break for family reasons', N'FAMBRK', 0),
    (N'994141D1-7BE2-B252-D956-CCD2ACDD6770', N'Other break', N'OTHBRK', 800),
    (N'43DD9BC5-1248-D5AA-2474-E2CEF23C9800', N'Other', N'OTHERR', 800),
    (N'22C4684B-9A03-F5CD-443D-BBEDC0571CCD', N'Not known', N'NOTKNW', 900),
    (N'C4D32D33-CF87-1AC3-6EA2-69FBBB7A21EF', N'Not Applicable - change of contract', N'NOTAPP', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[StaffOrigins] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'0C7BE67E-BF54-D2F5-3497-1F42A9837207'),(N'07AAD2D2-1A6E-DD4B-CBAA-E0F96749178A'),(N'51CB8C93-022B-A7B3-AF3E-75F4CDE8FEBB'),(N'908D7B4F-B26D-1848-08A2-5836AE6CCD56'),(N'2D4EFB06-CDCC-1D52-F377-5FE0637E9350'),(N'B5CAA084-6C1C-4BAB-30B9-B7048EAB2FF2'),(N'D63DE485-309D-D42B-56AA-DC58A06AF3F1'),(N'DC683D03-B680-7214-5C25-6CBE64327C5A'),(N'7B32B2E7-8A5F-C16C-3955-0AC443704842'),(N'C23CB96F-6120-40B1-0CE8-735951FD4C44'),(N'74C7CB11-FB61-1B0F-9C3C-D7C641BE80C1'),(N'C867FB2B-3BBF-F39F-6AD9-BDC71F4CC5BE'),(N'51B6C57C-A299-6E20-0F3B-AA31D4C40462'),(N'96B5C9B8-9A98-EA07-B667-D97CC2E69B5D'),(N'3FEBBCBC-691B-72FA-1F1F-BEE3E587E5E9'),(N'5706790A-309B-BEBF-E5D1-9772175BBDF3'),(N'F8D043F0-144B-019E-93A3-B99C9D643067'),(N'994141D1-7BE2-B252-D956-CCD2ACDD6770'),(N'43DD9BC5-1248-D5AA-2474-E2CEF23C9800'),(N'22C4684B-9A03-F5CD-443D-BBEDC0571CCD'),(N'C4D32D33-CF87-1AC3-6EA2-69FBBB7A21EF')) v([Id]));
GO

-- StaffDestinations (CS042, 24 rows) ---------------------------------
MERGE INTO [dbo].[StaffDestinations] AS Target
    USING (VALUES
    (N'2BBEFC4E-1803-333E-115A-625069C6F23E', N'Remaining in the same LA or MAT - primary school', N'LEAPRM', 0),
    (N'B76B7DF8-CE30-30EE-3C1D-E005E9326B13', N'Remaining in the same LA or MAT- secondary school', N'LEASEC', 0),
    (N'28D1D198-5492-57E4-2BFD-FF707177C0E5', N'Remaining in the same LA or MAT -other school types', N'LEASCH', 800),
    (N'8BBF93CE-DD6C-45BE-9049-A1C1E2C60BE3', N'Remaining in the same LA - or MAT - other (including central staff)', N'LEAOTH', 800),
    (N'04BC8D18-5AC4-3549-244F-80083F0F7437', N'Move to another LA or MAT- primary school', N'OTHPRM', 0),
    (N'3929470D-45A3-BA49-9479-8EB51F8CE79A', N'Move to another LA or MAT- secondary school', N'OTHSEC', 0),
    (N'25E998DC-70F1-7545-D6FE-7480126EC2C1', N'Move to another LA or MAT - other school types', N'OTHSCH', 800),
    (N'42E79D3C-A873-0CAA-3EC4-ED675F971A80', N'Move to another LA - other (including central staff)', N'OTHOTH', 800),
    (N'409CA901-387B-47E4-5BE5-06268283628D', N'Sixth form college - same LA area', N'LEASIX', 0),
    (N'8ED499CC-59B3-3564-2C36-9CE6B9DD067E', N'Sixth form college - other LA area', N'OTHSIX', 800),
    (N'A32D96D8-338A-F4E4-7A30-4D59EC8C6331', N'Independent school', N'INDEPN', 0),
    (N'796836DE-C8FC-894A-1D74-269DB2A03673', N'University, FE/HE college in UK', N'FHEHUK', 0),
    (N'2F02C353-7FA3-F11F-993C-856447897EF4', N'Education post outside UK', N'NONUKP', 0),
    (N'1B842F69-5EEA-F92C-C3A5-5A112D99A667', N'Non-Education post outside UK', N'NONUKO', 0),
    (N'ED8DB8EB-734F-EAAB-7568-3D24B1015A77', N'Non-education employment - public sector', N'PUBSEC', 0),
    (N'61084DA2-EF9F-0B42-7E8E-FB9C7E5B9550', N'Non-education employment - self-employment', N'SELFMP', 0),
    (N'037E3B51-43A8-401E-6688-B989368FD38E', N'Non-education employment - other employment', N'OTHERE', 800),
    (N'9DC6F9D8-D7B5-72F6-B382-6825FF82D0CF', N'Not Applicable - Change of Contract', N'NAPPCH', 900),
    (N'0BD27863-9D9E-443B-BC02-0F77021A1411', N'Other', N'OTHERR', 800),
    (N'FC276A09-5C0C-4C38-55D2-98BDCC2A6970', N'Not known', N'NTKNWN', 900),
    (N'F060DB25-5BA6-2654-6AAC-B7B76546FE66', N'Other education post in UK', N'OTHEDU', 800),
    (N'FAF183E8-0663-63F6-E5F5-1A82CB88D63E', N'Non-education employment - private sector', N'PRISEC', 0),
    (N'5B037AFC-0CEF-1F77-C519-0DAE06FFAAA3', N'Non-education employment - private sector (management or finance role)', N'PRIMAN', 0),
    (N'D74FD2FF-AF0C-CB56-78F7-93C602C93681', N'Non-education employment - self-employment (management or finance role)', N'SELMAN', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[StaffDestinations] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'2BBEFC4E-1803-333E-115A-625069C6F23E'),(N'B76B7DF8-CE30-30EE-3C1D-E005E9326B13'),(N'28D1D198-5492-57E4-2BFD-FF707177C0E5'),(N'8BBF93CE-DD6C-45BE-9049-A1C1E2C60BE3'),(N'04BC8D18-5AC4-3549-244F-80083F0F7437'),(N'3929470D-45A3-BA49-9479-8EB51F8CE79A'),(N'25E998DC-70F1-7545-D6FE-7480126EC2C1'),(N'42E79D3C-A873-0CAA-3EC4-ED675F971A80'),(N'409CA901-387B-47E4-5BE5-06268283628D'),(N'8ED499CC-59B3-3564-2C36-9CE6B9DD067E'),(N'A32D96D8-338A-F4E4-7A30-4D59EC8C6331'),(N'796836DE-C8FC-894A-1D74-269DB2A03673'),(N'2F02C353-7FA3-F11F-993C-856447897EF4'),(N'1B842F69-5EEA-F92C-C3A5-5A112D99A667'),(N'ED8DB8EB-734F-EAAB-7568-3D24B1015A77'),(N'61084DA2-EF9F-0B42-7E8E-FB9C7E5B9550'),(N'037E3B51-43A8-401E-6688-B989368FD38E'),(N'9DC6F9D8-D7B5-72F6-B382-6825FF82D0CF'),(N'0BD27863-9D9E-443B-BC02-0F77021A1411'),(N'FC276A09-5C0C-4C38-55D2-98BDCC2A6970'),(N'F060DB25-5BA6-2654-6AAC-B7B76546FE66'),(N'FAF183E8-0663-63F6-E5F5-1A82CB88D63E'),(N'5B037AFC-0CEF-1F77-C519-0DAE06FFAAA3'),(N'D74FD2FF-AF0C-CB56-78F7-93C602C93681')) v([Id]));
GO

-- ServiceChildIndicators (CS006, 4 rows) ----------------------------
MERGE INTO [dbo].[ServiceChildIndicators] AS Target
    USING (VALUES
    (N'2D638F3A-DD0B-97A5-E1CB-A24721F83B56', N'Yes', N'Y', 0),
    (N'36BD3891-3AC2-68A9-7185-E14000F932CC', N'No', N'N', 0),
    (N'C4A6CE28-E251-6C10-97FF-31C0D96F2281', N'Unknown', N'U', 900),
    (N'D08FDF0C-702D-C926-C7AD-CD8C07C43E3F', N'Refused', N'R', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[ServiceChildIndicators] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'2D638F3A-DD0B-97A5-E1CB-A24721F83B56'),(N'36BD3891-3AC2-68A9-7185-E14000F932CC'),(N'C4A6CE28-E251-6C10-97FF-31C0D96F2281'),(N'D08FDF0C-702D-C926-C7AD-CD8C07C43E3F')) v([Id]));
GO

-- YoungCarerIndicators (CS118, 3 rows) ------------------------------
MERGE INTO [dbo].[YoungCarerIndicators] AS Target
    USING (VALUES
    (N'BBD57F01-FC01-EEF2-E334-1A379BDADF3D', N'Identified as a young carer by parent or guardian', N'P', 0),
    (N'EEA85271-7EFF-F18C-D696-9F6F1C887157', N'Identified as a young carer by school', N'S', 0),
    (N'419EAD21-3DFD-9270-DCBF-34581AA815E6', N'Not declared (default value)', N'N', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[YoungCarerIndicators] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'BBD57F01-FC01-EEF2-E334-1A379BDADF3D'),(N'EEA85271-7EFF-F18C-D696-9F6F1C887157'),(N'419EAD21-3DFD-9270-DCBF-34581AA815E6')) v([Id]));
GO

-- PostLookedAfterArrangements (CS087, 6 rows) -----------------------
MERGE INTO [dbo].[PostLookedAfterArrangements] AS Target
    USING (VALUES
    (N'8D1E9DDC-9B07-C242-C2E2-B92B8F0D0338', N'Not declared', N'N', 900),
    (N'FE6AAB69-4C90-9D90-A238-AC763F1F0B64', N'Ceased to be looked after through adoption from England and Wales', N'A', 0),
    (N'4B267408-1ACE-2924-4C8A-5CC31524384A', N'Ceased to be looked after through a special guardianship order (SGO) from England and Wales', N'G', 0),
    (N'D00CA3A4-E075-C3BF-4F71-31974AD25E03', N'Ceased to be looked after through a residence order (RO) from England and Wales', N'R', 0),
    (N'348E9D59-1BA7-D94A-75F5-E8AED603AD2A', N'Ceased to be looked after through a child arrangement order (CAO) from England and Wales', N'C', 0),
    (N'DBED6C86-FCF2-93AD-4BF0-3437F0AF5D79', N'Ceased to be looked after through adoption from state care outside of England and Wales', N'O', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[PostLookedAfterArrangements] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'8D1E9DDC-9B07-C242-C2E2-B92B8F0D0338'),(N'FE6AAB69-4C90-9D90-A238-AC763F1F0B64'),(N'4B267408-1ACE-2924-4C8A-5CC31524384A'),(N'D00CA3A4-E075-C3BF-4F71-31974AD25E03'),(N'348E9D59-1BA7-D94A-75F5-E8AED603AD2A'),(N'DBED6C86-FCF2-93AD-4BF0-3437F0AF5D79')) v([Id]));
GO

-- UpnUnknownReasons (CS051, 10 rows) ---------------------------------
MERGE INTO [dbo].[UpnUnknownReasons] AS Target
    USING (VALUES
    (N'AB671791-51D7-86B5-BE8A-41C88EB21584', N'Child is aged under 6 years old and is not yet assigned a UPN.', N'UN1', 0),
    (N'EBFAC1E7-5B1A-E0A6-957C-E1EE7EF61216', N'Child has never attended a maintained school in England and has not been assigned a UPN.', N'UN2', 0),
    (N'F2B7EA2D-540E-E5F3-DD44-058FDBDEDF8C', N'Child is educated outside England and has not been assigned a UPN.', N'UN3', 0),
    (N'A0E6058A-EC3A-3570-2428-E5ACE9967E0A', N'Child is newly in need (one week before end of collection period) and the UPN was not yet known at the time of the CIN census annual statistical return.', N'UN4', 0),
    (N'A333A28F-9452-ADBA-BAFB-C68D717A9816', N'Sources collating UPNs reflect discrepancy/ies for the child''s name and/or surname and/or date of birth therefore preventing reliable matching (e.g. duplicated UPN).', N'UN5', 0),
    (N'E64758A0-1CDB-A64F-8BC7-11EFBB42DC64', N'Child is not looked after, and the authority is unable to obtain the UPN.', N'UN6', 0),
    (N'3FEFEFEF-A0B7-B94A-48EF-8EEEF2F632D7', N'Child Referred but No Further Action taken.', N'UN7', 0),
    (N'480B1BE2-D1DD-C2A5-C1E5-77DFAAD4F88D', N'Person is new to LA (one week before the end of the collection period) and the UPN or ULN is not yet known at the time of the SEN2 return', N'UN8', 0),
    (N'05719D22-372D-3170-6CBD-26537ADA8446', N'Young person has never attended a maintained school or further education setting in England and has not been assigned a UPN or ULN', N'UN9', 0),
    (N'68A03E3A-3095-7679-0088-2CD8F3C68D9F', N'Request for assessment resulted in no further action before UPN or ULN known', N'UN10', 0)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[UpnUnknownReasons] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'AB671791-51D7-86B5-BE8A-41C88EB21584'),(N'EBFAC1E7-5B1A-E0A6-957C-E1EE7EF61216'),(N'F2B7EA2D-540E-E5F3-DD44-058FDBDEDF8C'),(N'A0E6058A-EC3A-3570-2428-E5ACE9967E0A'),(N'A333A28F-9452-ADBA-BAFB-C68D717A9816'),(N'E64758A0-1CDB-A64F-8BC7-11EFBB42DC64'),(N'3FEFEFEF-A0B7-B94A-48EF-8EEEF2F632D7'),(N'480B1BE2-D1DD-C2A5-C1E5-77DFAAD4F88D'),(N'05719D22-372D-3170-6CBD-26537ADA8446'),(N'68A03E3A-3095-7679-0088-2CD8F3C68D9F')) v([Id]));
GO

-- EnglishProficiencies (CS089, 6 rows) ------------------------------
MERGE INTO [dbo].[EnglishProficiencies] AS Target
    USING (VALUES
    (N'3E9F555F-FE3D-83E6-00CB-920F1FA04C27', N'New to English', N'A', 0),
    (N'2D5E5447-5DFA-A5EF-3724-A77710E4B89D', N'Early Acquisition', N'B', 0),
    (N'9B15C463-C3CE-DFC1-254E-D505BA21F3FF', N'Developing competence', N'C', 0),
    (N'4A491263-4312-7CD2-09B3-DDB51B2CD8AE', N'Competent', N'D', 0),
    (N'E5917C01-6104-F76F-DD24-80571AB6FA39', N'Fluent', N'E', 0),
    (N'CB2B168F-D550-0BAB-8FC3-07F27765EF10', N'Not yet assessed', N'N', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[EnglishProficiencies] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'3E9F555F-FE3D-83E6-00CB-920F1FA04C27'),(N'2D5E5447-5DFA-A5EF-3724-A77710E4B89D'),(N'9B15C463-C3CE-DFC1-254E-D505BA21F3FF'),(N'4A491263-4312-7CD2-09B3-DDB51B2CD8AE'),(N'E5917C01-6104-F76F-DD24-80571AB6FA39'),(N'CB2B168F-D550-0BAB-8FC3-07F27765EF10')) v([Id]));
GO

-- KinshipCareIndicators (CS134, 4 rows) -----------------------------
MERGE INTO [dbo].[KinshipCareIndicators] AS Target
    USING (VALUES
    (N'7963BDD7-5274-3846-0136-453CC3044B8B', N'Not declared (default)', N'ND', 900),
    (N'EE7026D5-9823-AE92-4FAD-7230A238EF10', N'Kinship identified (formal arrangements)', N'KF', 0),
    (N'A03C6D06-80D1-C216-87EB-F0FA7DC0F504', N'Kinship identified (informal arrangements)', N'KI', 0),
    (N'393F591A-A895-60C0-F8DC-1575EE3517C9', N'Kinship identified (unknown under what arrangements)', N'KU', 900)
    ) AS Source (Id, Description, Code, DisplayOrder)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN INSERT (Id, Description, Code, DisplayOrder, Active) VALUES (Id, Description, Code, DisplayOrder, 1)
    WHEN MATCHED THEN UPDATE SET Description = Source.Description, Code = Source.Code, DisplayOrder = Source.DisplayOrder, Active = 1;
GO
UPDATE [dbo].[KinshipCareIndicators] SET [Active] = 0 WHERE [Id] NOT IN (SELECT [Id] FROM (VALUES (N'7963BDD7-5274-3846-0136-453CC3044B8B'),(N'EE7026D5-9823-AE92-4FAD-7230A238EF10'),(N'A03C6D06-80D1-C216-87EB-F0FA7DC0F504'),(N'393F591A-A895-60C0-F8DC-1575EE3517C9')) v([Id]));
GO
