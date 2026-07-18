SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's pre-employment checks. Full column list (incl. audit + version) so an update
-- round-trips without zeroing the created/audit columns; soft-deleted rows are excluded. 1:1.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_pre_employment_checks_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [IdentityCheckedDate], [ProhibitionFromTeachingCheckedDate],
        [ProhibitionFromManagementCheckedDate], [ChildcareDisqualificationCheckedDate],
        [MedicalFitnessCheckedDate], [QualificationsVerifiedDate], [Notes], [IsDeleted], [CreatedById],
        [CreatedByIpAddress], [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt],
        [Version] FROM [dbo].[StaffPreEmploymentChecks] WHERE [StaffMemberId] = @staffMemberId
        AND [IsDeleted] = 0;
END;
