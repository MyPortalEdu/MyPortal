SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's objectives. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_objective_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [ReviewId], [CategoryId], [Title], [Description], [SuccessCriteria],
        [DueDate], [StatusId], [ProgressNotes], [IsDeleted], [CreatedById], [CreatedByIpAddress], [CreatedAt],
        [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version] FROM [dbo].[StaffObjectives]
    WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;
END;
