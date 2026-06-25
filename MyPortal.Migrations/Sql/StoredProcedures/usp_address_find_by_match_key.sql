SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Find an existing address by its normalised match key (postcode + building number + building
-- name + street), case/whitespace-insensitive. Backs the server-side dedupe when a new address
-- is created — if one already exists we link it rather than insert a duplicate. Null if none.
CREATE OR ALTER PROCEDURE [dbo].[usp_address_find_by_match_key]
    @postcode NVARCHAR(128),
    @buildingNumber NVARCHAR(128),
    @buildingName NVARCHAR(128),
    @street NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

SELECT TOP 1
    [Id],
    [BuildingNumber],
    [BuildingName],
    [Apartment],
    [Street],
    [District],
    [Town],
    [County],
    [Postcode],
    [Country],
    [IsValidated],
    [IsDeleted],
    [CreatedById],
    [CreatedByIpAddress],
    [CreatedAt],
    [LastModifiedById],
    [LastModifiedByIpAddress],
    [LastModifiedAt],
    [Version]
FROM [dbo].[Addresses]
WHERE [IsDeleted] = 0
  AND REPLACE(UPPER(LTRIM(RTRIM([Postcode]))), ' ', '') = REPLACE(UPPER(LTRIM(RTRIM(@postcode))), ' ', '')
  AND UPPER(LTRIM(RTRIM([Street]))) = UPPER(LTRIM(RTRIM(@street)))
  AND UPPER(LTRIM(RTRIM(ISNULL([BuildingNumber], '')))) = UPPER(LTRIM(RTRIM(ISNULL(@buildingNumber, ''))))
  AND UPPER(LTRIM(RTRIM(ISNULL([BuildingName], '')))) = UPPER(LTRIM(RTRIM(ISNULL(@buildingName, ''))));
END
GO
