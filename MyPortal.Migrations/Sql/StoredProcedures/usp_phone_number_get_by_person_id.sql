SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- All non-deleted phone numbers owned by a person, mains first. Backs the
-- Contact Details area's load and whole-collection replace (entities, so the
-- service can diff and version-update them).
CREATE OR ALTER PROCEDURE [dbo].[usp_phone_number_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [Id],
    [TypeId],
    [PersonId],
    [AgencyId],
    [Number],
    [IsMain],
    [IsDeleted],
    [CreatedById],
    [CreatedByIpAddress],
    [CreatedAt],
    [LastModifiedById],
    [LastModifiedByIpAddress],
    [LastModifiedAt],
    [Version]
FROM [dbo].[PhoneNumbers]
WHERE [PersonId] = @personId AND [IsDeleted] = 0
ORDER BY [IsMain] DESC, [Number];
END
GO
