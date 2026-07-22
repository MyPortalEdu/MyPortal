-- ============================================================================
-- Teacher category & status — census classifications required for PLASC and
-- School Census returns:
--   TeacherCategories -> StaffMembers.TeacherCategoryId
--   TeacherStatuses   -> StaffMembers.TeacherStatusId
-- Both sit ALONGSIDE the existing HasQts flag rather than replacing it: the flag
-- answers "is this person qualified", the code answers "under which
-- classification", and the census needs the latter.
--
-- Also adds StaffMembers.EligibleForSwr — whether the person is included in the
-- School Workforce Return.
--
-- Same shape as the other CBDS-backed lookups (Code + DisplayOrder), but NOTE:
-- the DfE Codes are deliberately left NULL. The descriptions below are the
-- established classifications; the CBDS code strings must be populated from the
-- CBDS source before these are used to build a census return, rather than
-- guessed here. No new permissions — inside the existing ProfessionalDetails
-- area. Idempotent.
-- ============================================================================

IF OBJECT_ID(N'[dbo].[TeacherCategories]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TeacherCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_TeacherCategories_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_TeacherCategories_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_TeacherCategories PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF OBJECT_ID(N'[dbo].[TeacherStatuses]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TeacherStatuses] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL CONSTRAINT DF_TeacherStatuses_Active DEFAULT (1),
    [Code] nvarchar(10) NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_TeacherStatuses_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_TeacherStatuses PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF COL_LENGTH(N'[dbo].[StaffMembers]', N'TeacherCategoryId') IS NULL
    ALTER TABLE [dbo].[StaffMembers] ADD [TeacherCategoryId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffMembers]', N'TeacherStatusId') IS NULL
    ALTER TABLE [dbo].[StaffMembers] ADD [TeacherStatusId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffMembers]', N'EligibleForSwr') IS NULL
    ALTER TABLE [dbo].[StaffMembers] ADD [EligibleForSwr] bit NOT NULL
        CONSTRAINT DF_StaffMembers_EligibleForSwr DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_TeacherCategoryId_TeacherCategories'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
    ALTER TABLE [dbo].[StaffMembers] ADD CONSTRAINT [FK_StaffMembers_TeacherCategoryId_TeacherCategories]
        FOREIGN KEY ([TeacherCategoryId]) REFERENCES [dbo].[TeacherCategories]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_TeacherStatusId_TeacherStatuses'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
    ALTER TABLE [dbo].[StaffMembers] ADD CONSTRAINT [FK_StaffMembers_TeacherStatusId_TeacherStatuses]
        FOREIGN KEY ([TeacherStatusId]) REFERENCES [dbo].[TeacherStatuses]([Id]);
GO

-- ---- Seeds (Code left NULL — populate from CBDS before census use) ----
MERGE INTO dbo.TeacherCategories AS Target
USING (VALUES
    (N'7EAC4CA7-0000-4000-8000-000000000010', N'Qualified teacher',        10),
    (N'7EAC4CA7-0000-4000-8000-000000000020', N'Unqualified teacher',      20),
    (N'7EAC4CA7-0000-4000-8000-000000000030', N'Trainee teacher',          30),
    (N'7EAC4CA7-0000-4000-8000-000000000040', N'Instructor',               40),
    (N'7EAC4CA7-0000-4000-8000-000000000050', N'Overseas trained teacher', 50),
    (N'7EAC4CA7-0000-4000-8000-000000000060', N'Not a teacher',           800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1);
GO

MERGE INTO dbo.TeacherStatuses AS Target
USING (VALUES
    (N'7EAC5747-0000-4000-8000-000000000010', N'Qualified',      10),
    (N'7EAC5747-0000-4000-8000-000000000020', N'Unqualified',    20),
    (N'7EAC5747-0000-4000-8000-000000000030', N'Not applicable', 800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1);
GO
