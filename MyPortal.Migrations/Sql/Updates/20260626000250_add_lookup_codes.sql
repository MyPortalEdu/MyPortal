-- ============================================================================
-- Add a Code column to the description-only lookups that map to a DfE CBDS code
-- set, so statutory returns (School / Workforce Census) can emit the official
-- code rather than reverse-mapping from the (editable) description.
--
-- Religions and Disabilities are created without a Code column in
-- 20260626000100; ContractTypes predates that. Codes are populated from the
-- CBDS in 20260626000300 (runs after this).
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Religions') AND name = N'Code'
)
BEGIN
    ALTER TABLE dbo.Religions ADD Code nvarchar(10) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Disabilities') AND name = N'Code'
)
BEGIN
    ALTER TABLE dbo.Disabilities ADD Code nvarchar(10) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ContractTypes') AND name = N'Code'
)
BEGIN
    ALTER TABLE dbo.ContractTypes ADD Code nvarchar(10) NULL;
END
GO
