SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's DBS checks. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_dbs_check_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [DbsCheckTypeId], [CertificateNumber], [IssueDate], [ExpiryDate],
        [UpdateServiceEnrolled], [LastUpdateServiceCheck], [Notes], [IsDeleted], [CreatedById],
        [CreatedByIpAddress], [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt],
        [Version] FROM [dbo].[DbsChecks] WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;
END;
