SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- A person's non-deleted address links (entities). Used when reconciling the single-main rule:
-- the service loads these to clear IsMain on the others before setting a new main.
CREATE OR ALTER PROCEDURE [dbo].[usp_address_person_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [Id],
    [AddressId],
    [PersonId],
    [AddressTypeId],
    [IsMain],
    [IsDeleted],
    [CreatedById],
    [CreatedByIpAddress],
    [CreatedAt],
    [LastModifiedById],
    [LastModifiedByIpAddress],
    [LastModifiedAt],
    [Version]
FROM [dbo].[AddressPeople]
WHERE [PersonId] = @personId AND [IsDeleted] = 0;
END
GO
