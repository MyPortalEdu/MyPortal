SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Transitive line-management test: returns IsManaged = 1 when @managerStaffMemberId
-- appears anywhere ABOVE @subjectStaffMemberId in the LineManagerId chain (direct
-- manager, manager's manager, any depth), else 0. Backs StaffRelationship.LineManaged
-- in IStaffMemberAccessService — see docs/staff-profile-access.md.
--
-- A staff member never manages themselves. The Depth cap + MAXRECURSION guard against
-- cycles in bad data (A -> B -> A would otherwise recurse forever).
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_is_managed_by]
    @subjectStaffMemberId UNIQUEIDENTIFIER,
    @managerStaffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF @subjectStaffMemberId = @managerStaffMemberId
    BEGIN
        SELECT CAST(0 AS bit) AS IsManaged;
        RETURN;
    END

    ;WITH chain AS (
        -- Anchor: the subject's direct manager.
        SELECT sm.[LineManagerId] AS [AncestorId], 1 AS [Depth]
        FROM [dbo].[StaffMembers] sm
        WHERE sm.[Id] = @subjectStaffMemberId
          AND sm.[LineManagerId] IS NOT NULL

        UNION ALL

        -- Walk up: each ancestor's own manager.
        SELECT parent.[LineManagerId], c.[Depth] + 1
        FROM chain c
            INNER JOIN [dbo].[StaffMembers] parent ON parent.[Id] = c.[AncestorId]
        WHERE parent.[LineManagerId] IS NOT NULL
          AND c.[Depth] < 100
    )
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM chain WHERE [AncestorId] = @managerStaffMemberId)
                     THEN 1 ELSE 0 END AS bit) AS IsManaged
    OPTION (MAXRECURSION 100);
END
