
SELECT TOP 1
    la.[LeaCode]              AS [LaNumber],
    s.[EstablishmentNumber]  AS [Estab],
    s.[Urn]                  AS [Urn]
FROM [dbo].[Schools] s
    LEFT JOIN [dbo].[LocalAuthorities] la ON la.[Id] = s.[LocalAuthorityId];
