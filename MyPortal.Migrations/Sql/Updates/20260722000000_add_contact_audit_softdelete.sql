-- Promote Contacts to a managed CRUD entity (soft-delete + audit + version), matching Students /
-- StaffMembers, so the contact profile can soft-delete and track edits. Idempotent guarded column
-- adds; NOT NULL columns carry defaults so existing contact rows backfill cleanly.

IF COL_LENGTH(N'dbo.Contacts', N'IsDeleted') IS NULL
    ALTER TABLE dbo.Contacts ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Contacts_IsDeleted DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.Contacts', N'CreatedById') IS NULL
    ALTER TABLE dbo.Contacts ADD CreatedById UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Contacts_CreatedById DEFAULT ('00000000-0000-0000-0000-000000000000');
GO

IF COL_LENGTH(N'dbo.Contacts', N'CreatedByIpAddress') IS NULL
    ALTER TABLE dbo.Contacts ADD CreatedByIpAddress NVARCHAR(45) NOT NULL
        CONSTRAINT DF_Contacts_CreatedByIpAddress DEFAULT ('');
GO

IF COL_LENGTH(N'dbo.Contacts', N'CreatedAt') IS NULL
    ALTER TABLE dbo.Contacts ADD CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Contacts_CreatedAt DEFAULT (SYSUTCDATETIME());
GO

IF COL_LENGTH(N'dbo.Contacts', N'LastModifiedById') IS NULL
    ALTER TABLE dbo.Contacts ADD LastModifiedById UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Contacts_LastModifiedById DEFAULT ('00000000-0000-0000-0000-000000000000');
GO

IF COL_LENGTH(N'dbo.Contacts', N'LastModifiedByIpAddress') IS NULL
    ALTER TABLE dbo.Contacts ADD LastModifiedByIpAddress NVARCHAR(45) NOT NULL
        CONSTRAINT DF_Contacts_LastModifiedByIpAddress DEFAULT ('');
GO

IF COL_LENGTH(N'dbo.Contacts', N'LastModifiedAt') IS NULL
    ALTER TABLE dbo.Contacts ADD LastModifiedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Contacts_LastModifiedAt DEFAULT (SYSUTCDATETIME());
GO

IF COL_LENGTH(N'dbo.Contacts', N'Version') IS NULL
    ALTER TABLE dbo.Contacts ADD Version BIGINT NOT NULL CONSTRAINT DF_Contacts_Version DEFAULT (1);
GO
