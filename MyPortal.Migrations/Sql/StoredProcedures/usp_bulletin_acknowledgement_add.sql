SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Idempotent insert: the unique key on (BulletinId, UserId) means re-acks are
-- effectively no-ops. We use NOT EXISTS rather than swallowing a unique-violation
-- exception so the happy path stays exception-free under contention. Returns rows affected.
CREATE OR ALTER PROCEDURE [dbo].[usp_bulletin_acknowledgement_add]
    @bulletinId UNIQUEIDENTIFIER,
    @userId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.BulletinAcknowledgements (Id, BulletinId, UserId, AcknowledgedAt)
    SELECT NEWID(), @bulletinId, @userId, SYSUTCDATETIME()
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.BulletinAcknowledgements
        WHERE BulletinId = @bulletinId AND UserId = @userId
    );

    SELECT @@ROWCOUNT;
END;
GO
