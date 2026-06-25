SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- All non-deleted email addresses owned by a person, mains first. Backs the
-- Contact Details area's load and whole-collection replace (entities, so the
-- service can diff and version-update them).
CREATE OR ALTER PROCEDURE [dbo].[usp_email_address_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [Id],
    [TypeId],
    [PersonId],
    [AgencyId],
    [Address],
    [IsMain],
    [Notes],
    [IsDeleted],
    [CreatedById],
    [CreatedByIpAddress],
    [CreatedAt],
    [LastModifiedById],
    [LastModifiedByIpAddress],
    [LastModifiedAt],
    [Version]
FROM [dbo].[EmailAddresses]
WHERE [PersonId] = @personId AND [IsDeleted] = 0
ORDER BY [IsMain] DESC, [Address];
END
GO
