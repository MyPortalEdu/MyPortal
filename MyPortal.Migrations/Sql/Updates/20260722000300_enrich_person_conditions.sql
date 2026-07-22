-- Enrich the pupil condition record (PersonConditions): clarify the medication flag name and add
-- onset / resolved / info-received dates and free-text notes. Idempotent + guarded.

-- Rename MedicationTaken -> RequiresMedication (only if the old column is still there).
IF COL_LENGTH(N'dbo.PersonConditions', N'MedicationTaken') IS NOT NULL
   AND COL_LENGTH(N'dbo.PersonConditions', N'RequiresMedication') IS NULL
    EXEC sp_rename N'dbo.PersonConditions.MedicationTaken', N'RequiresMedication', N'COLUMN';
GO

IF COL_LENGTH(N'dbo.PersonConditions', N'StartDate') IS NULL
    ALTER TABLE dbo.PersonConditions ADD StartDate DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.PersonConditions', N'EndDate') IS NULL
    ALTER TABLE dbo.PersonConditions ADD EndDate DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.PersonConditions', N'InfoReceivedDate') IS NULL
    ALTER TABLE dbo.PersonConditions ADD InfoReceivedDate DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.PersonConditions', N'Notes') IS NULL
    ALTER TABLE dbo.PersonConditions ADD Notes NVARCHAR(MAX) NULL;
GO
