-- Continuous-service and local-authority start dates on the employment spell, plus the free-text
-- previous/next employer names that sit alongside the coded Origin/Destination census items.
-- Continuous service can predate the spell: it carries in from an earlier employer for redundancy
-- and pension entitlement, so it is deliberately not constrained to StartDate. Idempotent.

IF COL_LENGTH(N'[dbo].[StaffEmployments]', N'ContinuousServiceStartDate') IS NULL
    ALTER TABLE [dbo].[StaffEmployments] ADD [ContinuousServiceStartDate] datetime2(7) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffEmployments]', N'LocalAuthorityStartDate') IS NULL
    ALTER TABLE [dbo].[StaffEmployments] ADD [LocalAuthorityStartDate] datetime2(7) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffEmployments]', N'PreviousEmployer') IS NULL
    ALTER TABLE [dbo].[StaffEmployments] ADD [PreviousEmployer] nvarchar(128) NULL;
GO

IF COL_LENGTH(N'[dbo].[StaffEmployments]', N'NextEmployer') IS NULL
    ALTER TABLE [dbo].[StaffEmployments] ADD [NextEmployer] nvarchar(128) NULL;
GO
