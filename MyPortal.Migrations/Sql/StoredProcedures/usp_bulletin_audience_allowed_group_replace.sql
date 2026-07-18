SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Replace the school-wide bulletin audience allowlist wholesale: clear it, then insert the
-- supplied StudentGroup ids. Caller wraps this in a transaction so the DELETE + INSERT are
-- atomic (a failure mid-way must not leave the allowlist empty). An empty @studentGroupIds
-- therefore clears the list, which is the intended "no restrictions configured" state.
--
-- Single-tenant assumption: the allowlist is school-wide, so the unscoped DELETE is correct.
-- If multi-tenancy is added, this table needs a SchoolId column and the DELETE must be scoped.
CREATE OR ALTER PROCEDURE [dbo].[usp_bulletin_audience_allowed_group_replace]
    @studentGroupIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.BulletinAudienceAllowedGroups;

    INSERT INTO dbo.BulletinAudienceAllowedGroups (StudentGroupId)
    SELECT [Value] FROM @studentGroupIds;
END;
