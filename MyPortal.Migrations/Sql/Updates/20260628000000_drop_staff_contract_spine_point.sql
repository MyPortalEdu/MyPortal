-- ============================================================================
-- Drop the redundant free-text StaffContracts.SpinePoint column. The structured
-- PayScalePointId (a lookup that drives statutory salary) is the single source
-- of spine-point data; the free-text field fed no logic and duplicated the label.
-- Idempotent.
-- ============================================================================

IF COL_LENGTH(N'dbo.StaffContracts', N'SpinePoint') IS NOT NULL
    ALTER TABLE [dbo].[StaffContracts] DROP COLUMN [SpinePoint];
GO
