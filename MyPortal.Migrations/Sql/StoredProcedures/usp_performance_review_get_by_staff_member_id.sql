SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A staff member's performance reviews. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_performance_review_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StaffMemberId], [CycleName], [ReviewerId], [StatusId], [ReviewDate], [NextReviewDate],
        [OverallOutcomeId], [Summary], [IsDeleted], [CreatedById], [CreatedByIpAddress], [CreatedAt],
        [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version]
    FROM [dbo].[PerformanceReviews] WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;
END;
