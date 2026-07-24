SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's linked disabilities.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_disability_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [DisabilityId], [DateAdvised], [IsLongTerm], [AffectsWorkingAbility],
        [AssistanceRequired] FROM [dbo].[StaffMemberDisabilities]
    WHERE [StaffMemberId] = @staffMemberId;
END;
