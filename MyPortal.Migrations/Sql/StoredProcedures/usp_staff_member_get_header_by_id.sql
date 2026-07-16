SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Staff profile header — identity row shown at the top of the page, plus the
-- bits the service needs to derive status. DisplayName uses fn_person_get_name
-- format 1 (Title First [Middle] Last) on the legal name; PreferredName is the
-- preferred first name (or null).
--
-- The three employment flags let the service derive the lifecycle badge
-- (Active / Future / Leaver / None) without loading the gated Employment section:
-- they are date aggregates over the member's non-deleted spells as of today, not
-- the salary/contract detail. Date-only comparison; an open-ended spell counts as
-- current. IsDeleted (Archived) takes precedence over all of them in the service.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_header_by_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today date = CAST(SYSUTCDATETIME() AS date);

SELECT
    [SM].[Id],
    [SM].[PersonId],
    [SM].[Code],
    [SM].[IsDeleted],
    [N].[Name] AS [DisplayName],
    NULLIF([P].[PreferredFirstName], '') AS [PreferredName],
    [P].[PhotoId],
    CONVERT(bit, CASE WHEN EXISTS (
        SELECT 1 FROM [dbo].[StaffEmployments] [E]
        WHERE [E].[StaffMemberId] = [SM].[Id] AND [E].[IsDeleted] = 0
          AND CAST([E].[StartDate] AS date) <= @today
          AND ([E].[EndDate] IS NULL OR CAST([E].[EndDate] AS date) >= @today)
    ) THEN 1 ELSE 0 END) AS [HasCurrentEmployment],
    CONVERT(bit, CASE WHEN EXISTS (
        SELECT 1 FROM [dbo].[StaffEmployments] [E]
        WHERE [E].[StaffMemberId] = [SM].[Id] AND [E].[IsDeleted] = 0
          AND CAST([E].[StartDate] AS date) > @today
    ) THEN 1 ELSE 0 END) AS [HasFutureEmployment],
    CONVERT(bit, CASE WHEN EXISTS (
        SELECT 1 FROM [dbo].[StaffEmployments] [E]
        WHERE [E].[StaffMemberId] = [SM].[Id] AND [E].[IsDeleted] = 0
    ) THEN 1 ELSE 0 END) AS [HasAnyEmployment]
FROM [dbo].[StaffMembers] [SM]
    INNER JOIN [dbo].[People] [P] ON [SM].[PersonId] = [P].[Id]
    CROSS APPLY [dbo].[fn_person_get_name]([SM].[PersonId], 1, 0, 1) [N]
WHERE [SM].[Id] = @staffMemberId;
END
