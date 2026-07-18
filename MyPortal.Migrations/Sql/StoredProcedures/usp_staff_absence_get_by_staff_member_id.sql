SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's absences. Lean record table (no audit / soft-delete); rows are hard-deleted on
-- reconcile.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_absence_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [AbsenceTypeId], [IllnessTypeId], [StartDate], [EndDate],
        [IsConfidential], [Notes] FROM [dbo].[StaffAbsences] WHERE [StaffMemberId] = @staffMemberId;
END;
