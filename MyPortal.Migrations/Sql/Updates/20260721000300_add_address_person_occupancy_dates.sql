-- Address occupancy dates on the person↔address link (SIMS residence StartDate/EndDate), so a
-- person's address history / moves can be tracked. Both nullable — an open-ended current address
-- leaves them NULL. Idempotent guarded column adds.

IF COL_LENGTH(N'dbo.AddressPeople', N'StartDate') IS NULL
    ALTER TABLE dbo.AddressPeople ADD StartDate DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.AddressPeople', N'EndDate') IS NULL
    ALTER TABLE dbo.AddressPeople ADD EndDate DATETIME2 NULL;
GO
