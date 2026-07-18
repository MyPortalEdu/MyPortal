SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's qualifications. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_qualification_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [QualificationLevelId], [Title], [Subject], [AwardingBody],
        [Grade], [ClassOfDegreeId], [YearAwarded], [IsDeleted], [CreatedById], [CreatedByIpAddress],
        [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version]
    FROM [dbo].[StaffQualifications] WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;
END;
