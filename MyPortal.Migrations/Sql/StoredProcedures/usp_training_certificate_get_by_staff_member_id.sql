SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's training certificates. Lean record table (no audit / soft-delete); rows are
-- hard-deleted on reconcile.
CREATE OR ALTER PROCEDURE [dbo].[usp_training_certificate_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [TrainingCourseId], [StaffMemberId], [TrainingCertificateStatusId], [CompletedDate],
        [ExpiryDate], [Provider], [Hours], [CertificateReference] FROM [dbo].[TrainingCertificates]
    WHERE [StaffMemberId] = @staffMemberId;
END;
