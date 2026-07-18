SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_bulletin_audience_allowed_group_get_all]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SG.Id          AS StudentGroupId,
        SG.Code        AS Code,
        SG.Description AS Name
    FROM dbo.BulletinAudienceAllowedGroups AAG
    JOIN dbo.StudentGroups                  SG ON SG.Id = AAG.StudentGroupId
    ORDER BY SG.Description;
END;
GO
