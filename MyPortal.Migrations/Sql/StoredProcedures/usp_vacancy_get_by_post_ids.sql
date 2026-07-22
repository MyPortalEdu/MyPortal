SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Vacancies for a set of established posts. Full column list (incl. audit + version) so reconcile
-- updates round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_vacancy_get_by_post_ids]
    @postIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.[Id], V.[PostId], V.[StartDate], V.[EndDate], V.[IsAdvertised], V.[IsTemporarilyFilled],
        V.[SubjectId], V.[Notes], V.[IsDeleted], V.[CreatedById], V.[CreatedByIpAddress], V.[CreatedAt],
        V.[LastModifiedById], V.[LastModifiedByIpAddress], V.[LastModifiedAt], V.[Version]
    FROM [dbo].[Vacancies] AS V
        INNER JOIN @postIds AS PI ON PI.[Value] = V.[PostId]
    WHERE V.[IsDeleted] = 0
    ORDER BY V.[StartDate] DESC;
END;
