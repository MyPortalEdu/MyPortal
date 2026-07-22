SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- A person's linked addresses (mains first) joined to the shared Address rows, with a count of
-- how many people share each address. Backs the addresses section of the Contact Details area.
CREATE OR ALTER PROCEDURE [dbo].[usp_address_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [AP].[Id] AS [AddressPersonId],
    [A].[Id] AS [AddressId],
    [AP].[AddressTypeId] AS [TypeId],
    [AP].[IsMain],
    [AP].[StartDate],
    [AP].[EndDate],
    [A].[BuildingNumber],
    [A].[BuildingName],
    [A].[Apartment],
    [A].[Street],
    [A].[District],
    [A].[Town],
    [A].[County],
    [A].[Postcode],
    [A].[Country],
    (SELECT COUNT(*) FROM [dbo].[AddressPeople] [X]
        WHERE [X].[AddressId] = [A].[Id] AND [X].[IsDeleted] = 0) AS [SharedCount]
FROM [dbo].[AddressPeople] [AP]
    INNER JOIN [dbo].[Addresses] [A] ON [A].[Id] = [AP].[AddressId] AND [A].[IsDeleted] = 0
WHERE [AP].[PersonId] = @personId AND [AP].[IsDeleted] = 0
ORDER BY [AP].[IsMain] DESC, [A].[Postcode];
END
GO
