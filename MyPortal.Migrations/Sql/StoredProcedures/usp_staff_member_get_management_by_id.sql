SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Management section of the staff profile: result set 1 is the subject's CURRENT line manager
-- (null when unmanaged today); result set 2 is their current direct reports. Both resolve
-- against the date-ranged dbo.StaffLineManagers — the row covering today wins, so an ended
-- reporting line drops out of both. Names via the shared person-name function (format 3 =
-- First [Middle] Last, preferred names on). Soft-deleted rows excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_management_by_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today DATETIME2(7) = CAST(CAST(SYSUTCDATETIME() AS DATE) AS DATETIME2(7));

    ;WITH current_line AS (
        SELECT [StaffMemberId], [LineManagerId],
               ROW_NUMBER() OVER (PARTITION BY [StaffMemberId] ORDER BY [StartDate] DESC) AS [Rn]
        FROM [dbo].[StaffLineManagers]
        WHERE [IsDeleted] = 0
          AND [StartDate] <= @today
          AND ([EndDate] IS NULL OR [EndDate] >= @today)
    )
    SELECT
        [CL].[LineManagerId],
        [MgrName].[Name] AS [LineManagerName],
        [Mgr].[Code]     AS [LineManagerCode]
    FROM [dbo].[StaffMembers] [SM]
        LEFT JOIN current_line [CL] ON [CL].[StaffMemberId] = [SM].[Id] AND [CL].[Rn] = 1
        LEFT JOIN [dbo].[StaffMembers] [Mgr]
            ON [Mgr].[Id] = [CL].[LineManagerId] AND [Mgr].[IsDeleted] = 0
        OUTER APPLY [dbo].[fn_person_get_name]([Mgr].[PersonId], 3, 1, 0) AS [MgrName]
    WHERE [SM].[Id] = @staffMemberId;

    ;WITH current_line AS (
        SELECT [StaffMemberId], [LineManagerId],
               ROW_NUMBER() OVER (PARTITION BY [StaffMemberId] ORDER BY [StartDate] DESC) AS [Rn]
        FROM [dbo].[StaffLineManagers]
        WHERE [IsDeleted] = 0
          AND [StartDate] <= @today
          AND ([EndDate] IS NULL OR [EndDate] >= @today)
    )
    SELECT
        [R].[Id]       AS [StaffMemberId],
        [RName].[Name] AS [DisplayName],
        [R].[Code]
    FROM current_line [CL]
        INNER JOIN [dbo].[StaffMembers] [R] ON [R].[Id] = [CL].[StaffMemberId] AND [R].[IsDeleted] = 0
        OUTER APPLY [dbo].[fn_person_get_name]([R].[PersonId], 3, 1, 0) AS [RName]
    WHERE [CL].[LineManagerId] = @staffMemberId AND [CL].[Rn] = 1
    ORDER BY [RName].[Name];
END
