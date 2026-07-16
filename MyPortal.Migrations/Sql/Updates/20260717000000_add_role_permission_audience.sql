-- ============================================================================
-- Add an audience (UserType) dimension to Permissions and Roles, plus an
-- IsDefault flag on Roles.
--
-- UserType (MyPortal.Common.Enums.UserType: Staff=1, Student=2, Parent=3)
-- segregates permissions and roles by portal — a Student role may only hold
-- Student permissions, etc. Every existing row is staff-facing, so the DEFAULT
-- backfills them to Staff (1).
--
-- IsDefault marks a seeded role whose identity is protected (no delete/rename)
-- but whose permission grants stay school-editable — distinct from IsSystem,
-- which locks the row entirely. Enforced in RoleService.
--
-- Idempotent guarded column adds. DEFAULT constraints are kept (matching the
-- repo pattern) so any raw INSERT that omits the column lands on Staff/false.
-- ============================================================================

IF COL_LENGTH(N'dbo.Permissions', N'UserType') IS NULL
    ALTER TABLE dbo.Permissions
        ADD UserType INT NOT NULL CONSTRAINT DF_Permissions_UserType DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.Roles', N'UserType') IS NULL
    ALTER TABLE dbo.Roles
        ADD UserType INT NOT NULL CONSTRAINT DF_Roles_UserType DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.Roles', N'IsDefault') IS NULL
    ALTER TABLE dbo.Roles
        ADD IsDefault BIT NOT NULL CONSTRAINT DF_Roles_IsDefault DEFAULT (0);
GO
