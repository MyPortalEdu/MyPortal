SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_school_get_details_local]
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [S].[Id],
    [S].[AgencyId],
    [A].[Name],
    [A].[Website],
    [A].[AgencyTypeId],
    [AT].[Description] AS [AgencyType],
    [S].[Urn],
    [S].[Uprn],
    [S].[EstablishmentNumber],
    [S].[LocalAuthorityId],
    [LA].[Name] AS [LocalAuthorityName],
    [S].[SchoolPhaseId],
    [SP].[Description] AS [Phase],
    [S].[SchoolTypeId],
    [ST].[Description] AS [Type],
    [S].[GovernanceTypeId],
    [GT].[Description] AS [GovernanceType],
    [S].[IntakeTypeId],
    [IT].[Description] AS [IntakeType],
    [HT].[Name] AS [HeadTeacherFullName],
    [S].[IsLocal]

FROM [dbo].[Schools] [S]
    INNER JOIN [Agencies] [A] ON [S].[AgencyId] = [A].[Id]
    INNER JOIN [AgencyTypes] [AT] ON [A].[AgencyTypeId] = [AT].[Id]
    LEFT JOIN [LocalAuthorities] [LA] ON [S].[LocalAuthorityId] = [LA].[Id]
    INNER JOIN [SchoolPhases] [SP] ON [S].[SchoolPhaseId] = [SP].[Id]
    INNER JOIN [SchoolTypes] [ST] ON [S].[SchoolTypeId] = [ST].[Id]
    INNER JOIN [GovernanceTypes] [GT] ON [S].[GovernanceTypeId] = [GT].[Id]
    INNER JOIN [IntakeTypes] [IT] ON [S].[IntakeTypeId] = [IT].[Id]
    CROSS APPLY [fn_person_get_name]([S].[HeadTeacherId], 2, 0 ,1) HT

WHERE [S].[IsLocal] = 1
END;