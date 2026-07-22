-- Equality Act disability detail: per-declared-disability advised date, long-term and
-- affects-working-ability flags, and the agreed adjustments; plus the day-to-day-activities
-- effect determinant and registered disability number. Idempotent.

IF OBJECT_ID(N'[dbo].[ImpairmentEffects]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ImpairmentEffects] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [DisplayOrder] int NOT NULL CONSTRAINT DF_ImpairmentEffects_DisplayOrder DEFAULT (0),
    [IsSystem] bit NOT NULL CONSTRAINT DF_ImpairmentEffects_IsSystem DEFAULT (0),
    [Active] bit NOT NULL CONSTRAINT DF_ImpairmentEffects_Active DEFAULT (1),
    CONSTRAINT PK_ImpairmentEffects PRIMARY KEY CLUSTERED ([Id])
);
END
GO

-- ---- Per-declared-disability detail ----
IF COL_LENGTH(N'[dbo].[StaffMemberDisabilities]', N'DateAdvised') IS NULL
    ALTER TABLE [dbo].[StaffMemberDisabilities] ADD [DateAdvised] datetime2(7) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffMemberDisabilities]', N'IsLongTerm') IS NULL
    ALTER TABLE [dbo].[StaffMemberDisabilities] ADD [IsLongTerm] bit NOT NULL
        CONSTRAINT DF_StaffMemberDisabilities_IsLongTerm DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[StaffMemberDisabilities]', N'AffectsWorkingAbility') IS NULL
    ALTER TABLE [dbo].[StaffMemberDisabilities] ADD [AffectsWorkingAbility] bit NOT NULL
        CONSTRAINT DF_StaffMemberDisabilities_AffectsWorkingAbility DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[StaffMemberDisabilities]', N'AssistanceRequired') IS NULL
    ALTER TABLE [dbo].[StaffMemberDisabilities] ADD [AssistanceRequired] nvarchar(512) NULL;
GO

-- ---- Staff-level Equality Act determinant + registered number ----
IF COL_LENGTH(N'[dbo].[StaffMembers]', N'ImpairmentEffectId') IS NULL
    ALTER TABLE [dbo].[StaffMembers] ADD [ImpairmentEffectId] uniqueidentifier NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffMembers]', N'DisabilityNumber') IS NULL
    ALTER TABLE [dbo].[StaffMembers] ADD [DisabilityNumber] nvarchar(32) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StaffMembers_ImpairmentEffectId_ImpairmentEffects'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffMembers]'))
    ALTER TABLE [dbo].[StaffMembers] ADD CONSTRAINT [FK_StaffMembers_ImpairmentEffectId_ImpairmentEffects]
        FOREIGN KEY ([ImpairmentEffectId]) REFERENCES [dbo].[ImpairmentEffects]([Id]);
GO

-- ---- Seed ----
MERGE INTO dbo.ImpairmentEffects AS Target
USING (VALUES
    (N'E44EC700-0000-4000-8000-000000000010', N'Yes — substantially affects day-to-day activities', 10),
    (N'E44EC700-0000-4000-8000-000000000020', N'No — does not substantially affect day-to-day activities', 20),
    (N'E44EC700-0000-4000-8000-000000000030', N'Not yet assessed',                                    30),
    (N'E44EC700-0000-4000-8000-000000000040', N'Prefer not to say',                                  800)
) AS Source (Id, Description, DisplayOrder)
    ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET Description = Source.Description, DisplayOrder = Source.DisplayOrder, IsSystem = 1, Active = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Description, DisplayOrder, IsSystem, Active)
    VALUES (Source.Id, Source.Description, Source.DisplayOrder, 1, 1);
GO
