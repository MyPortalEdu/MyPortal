-- ============================================================================
-- Align Person.Gender to the DfE CBDS Sex code set (CS119): the legacy "Unknown"
-- code 'X' becomes 'U'. M/F are unchanged. Idempotent.
-- ============================================================================

UPDATE [dbo].[People] SET [Gender] = 'U' WHERE [Gender] = 'X';
GO
