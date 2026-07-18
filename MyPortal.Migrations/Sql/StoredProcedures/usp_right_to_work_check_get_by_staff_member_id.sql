SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's right-to-work checks. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_right_to_work_check_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [DocumentTypeId], [VerifiedById], [DocumentNumber], [CheckDate],
        [DocumentExpiryDate], [FollowUpDate], [Notes], [IsDeleted], [CreatedById], [CreatedByIpAddress],
        [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version]
    FROM [dbo].[RightToWorkChecks] WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;
END;
