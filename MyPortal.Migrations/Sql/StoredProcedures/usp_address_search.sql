SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Search existing (non-deleted) addresses for the search-before-add flow. @like is the
-- caller-built contains pattern; matches postcode / street / building / town. Each row carries a
-- count of people already linked. Capped at 25.
CREATE OR ALTER PROCEDURE [dbo].[usp_address_search]
    @like NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

SELECT TOP 25
    [A].[Id] AS [AddressId],
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
        WHERE [X].[AddressId] = [A].[Id] AND [X].[IsDeleted] = 0) AS [LinkedPersonCount]
FROM [dbo].[Addresses] [A]
WHERE [A].[IsDeleted] = 0
  AND ( [A].[Postcode] LIKE @like
     OR [A].[Street] LIKE @like
     OR [A].[BuildingName] LIKE @like
     OR [A].[BuildingNumber] LIKE @like
     OR [A].[Town] LIKE @like )
ORDER BY [A].[Postcode], [A].[Street];
END
GO
