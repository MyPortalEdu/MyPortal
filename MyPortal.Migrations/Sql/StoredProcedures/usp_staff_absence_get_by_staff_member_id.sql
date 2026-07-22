SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's absences. Lean record table (no audit / soft-delete); rows are hard-deleted on
-- reconcile. Includes the statutory / payroll treatment columns (authorised pay rate, payroll reason,
-- SSP exclusion, days & hours lost, industrial injury) — those are HR-only at the service layer.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_absence_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [AbsenceTypeId], [IllnessTypeId], [StartDate], [EndDate],
        [IsConfidential], [Notes], [AuthorisedPayRateId], [PayrollReasonId], [SspExcluded],
        [WorkingDaysLost], [HoursLost], [IsIndustrialInjury]
    FROM [dbo].[StaffAbsences] WHERE [StaffMemberId] = @staffMemberId;
END;
