SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Single-bit visibility check for a bulletin. Mirrors the predicate in
-- usp_bulletin_get_details_by_id so attachment authorisation paths
-- (CanViewDirectoryAsync / CanViewDocumentAsync) can confirm audience
-- membership without taking the full details round-trip. Returns one
-- row, one column @IsVisible BIT.
--
-- Visibility predicate (must match the details SP exactly):
--   • staff pinners see everything;
--   • staff creators see their own (even after expiry);
--   • otherwise the caller must be in the audience AND the bulletin
--     must not be expired.
CREATE OR ALTER PROCEDURE [dbo].[usp_bulletin_is_visible]
    @bulletinId    UNIQUEIDENTIFIER,
    @currentUserId UNIQUEIDENTIFIER,
    @isStaff       BIT,
    @isPupil       BIT,
    @isParent      BIT,
    @canView       BIT,
    @canEdit       BIT,
    @canPin        BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @nowUtc DATETIME2(7) = SYSUTCDATETIME();

    ;WITH PupilGroups AS (
        SELECT SGM.StudentGroupId
        FROM dbo.StudentGroupMemberships SGM
        JOIN dbo.Students                S   ON S.Id = SGM.StudentId
        JOIN dbo.Users                   U   ON U.PersonId = S.PersonId
        WHERE @isPupil = 1
          AND U.Id = @currentUserId
          AND SGM.StartDate <= @nowUtc
          AND (SGM.EndDate IS NULL OR SGM.EndDate > @nowUtc)
    ),
    ParentGroups AS (
        SELECT SGM.StudentGroupId
        FROM dbo.StudentGroupMemberships     SGM
        JOIN dbo.Students                    S   ON S.Id   = SGM.StudentId
        JOIN dbo.StudentContactRelationships SCR ON SCR.StudentId = S.Id
        JOIN dbo.Contacts                    C   ON C.Id   = SCR.ContactId
        JOIN dbo.Users                       U   ON U.PersonId = C.PersonId
        WHERE @isParent = 1
          AND U.Id = @currentUserId
          AND SGM.StartDate <= @nowUtc
          AND (SGM.EndDate IS NULL OR SGM.EndDate > @nowUtc)
    )
    SELECT IsVisible = CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Bulletins B
            WHERE B.Id = @bulletinId
              AND (
                    (@isStaff = 1 AND @canPin = 1)
                 OR (@isStaff = 1 AND @canEdit = 1 AND B.CreatedById = @currentUserId)
                 OR (
                        (B.ExpiresAt IS NULL OR B.ExpiresAt > @nowUtc)
                        AND EXISTS (
                            SELECT 1
                            FROM dbo.BulletinAudiences BA
                            WHERE BA.BulletinId = B.Id
                              AND (
                                    (@isStaff  = 1 AND BA.AudienceKind = 1)
                                 OR (@isPupil  = 1 AND BA.AudienceKind = 2)
                                 OR (@isParent = 1 AND BA.AudienceKind = 3)
                                 OR (@isPupil  = 1 AND BA.AudienceKind = 4
                                                    AND BA.StudentGroupId IN (SELECT StudentGroupId FROM PupilGroups))
                                 OR (@isParent = 1 AND BA.AudienceKind = 4
                                                    AND BA.StudentGroupId IN (SELECT StudentGroupId FROM ParentGroups))
                              )
                        )
                    )
              )
        ) THEN 1 ELSE 0 END
    AS BIT);
END;
