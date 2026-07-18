SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if another (non-deleted) staff member already uses this code. Staff codes must be
-- unique across the school; a soft-deleted member's code is free to reuse. Pass
-- @excludeStaffMemberId on update so a member doesn't clash with itself.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_code_exists]
    @code                 NVARCHAR(128),
    @excludeStaffMemberId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.StaffMembers AS SM
        WHERE SM.Code = @code
          AND SM.IsDeleted = 0
          AND (@excludeStaffMemberId IS NULL OR SM.Id <> @excludeStaffMemberId)
    ) THEN 1 ELSE 0 END AS bit);
END;
GO
