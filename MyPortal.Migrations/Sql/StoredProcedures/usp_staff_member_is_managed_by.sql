SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Transitive line-management test: returns IsManaged = 1 when @managerStaffMemberId
-- appears anywhere ABOVE @subjectStaffMemberId in the reporting chain (direct
-- manager, manager's manager, any depth), else 0. Backs StaffRelationship.LineManaged
-- in IStaffMemberAccessService — see docs/staff-profile-access.md.
--
-- The chain resolves against dbo.StaffLineManagers, which is date-ranged: only the row
-- current TODAY counts, so a manager whose period has ended no longer confers access.
-- (StaffMembers.LineManagerId is retained as a convenience copy but is NOT authoritative.)
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

    DECLARE @today DATETIME2(7) = CAST(CAST(SYSUTCDATETIME() AS DATE) AS DATETIME2(7));

    ;WITH current_line AS (
        -- One current manager per staff member: the latest-starting row covering today.
        SELECT [StaffMemberId], [LineManagerId],
               ROW_NUMBER() OVER (PARTITION BY [StaffMemberId] ORDER BY [StartDate] DESC) AS [Rn]
        FROM [dbo].[StaffLineManagers]
        WHERE [IsDeleted] = 0
          AND [StartDate] <= @today
          AND ([EndDate] IS NULL OR [EndDate] >= @today)
    ),
    chain AS (
        -- Anchor: the subject's current manager.
        SELECT cl.[LineManagerId] AS [AncestorId], 1 AS [Depth]
        FROM current_line cl
        WHERE cl.[StaffMemberId] = @subjectStaffMemberId AND cl.[Rn] = 1

        UNION ALL

        -- Walk up: each ancestor's own current manager.
        SELECT parent.[LineManagerId], c.[Depth] + 1
        FROM chain c
            INNER JOIN current_line parent
                ON parent.[StaffMemberId] = c.[AncestorId] AND parent.[Rn] = 1
        WHERE c.[Depth] < 100
    )
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM chain WHERE [AncestorId] = @managerStaffMemberId)
                     THEN 1 ELSE 0 END AS bit) AS IsManaged
    OPTION (MAXRECURSION 100);
END
