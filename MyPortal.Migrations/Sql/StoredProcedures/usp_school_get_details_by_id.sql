SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_school_get_details_by_id]
    @schoolId UNIQUEIDENTIFIER
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
    [S].[HeadTeacherId],
    [HT].[Name] AS [HeadTeacherFullName],
    [S].[Ukprn],
    [S].[PayZoneId],
    [PZ].[Description] AS [PayZoneName],
    [S].[LowestAge],
    [S].[HighestAge],
    [S].[NetCapacity],
    [S].[NetCapacityAssessmentDate],
    [S].[IsSpecialSchool],
    [S].[SpecialSchoolOrganisationId],
    [SSO].[Description] AS [SpecialSchoolOrganisationName],
    [S].[SpecialSchoolTypeId],
    [SST].[Description] AS [SpecialSchoolTypeName],
    [S].[MaxBoarders],
    [S].[Telephone],
    [S].[Email],
    [S].[IsLocal]

FROM [dbo].[Schools] [S]
    INNER JOIN [dbo].[Agencies] [A] ON [S].[AgencyId] = [A].[Id]
    INNER JOIN [dbo].[AgencyTypes] [AT] ON [A].[AgencyTypeId] = [AT].[Id]
    LEFT JOIN [dbo].[LocalAuthorities] [LA] ON [S].[LocalAuthorityId] = [LA].[Id]
    INNER JOIN [dbo].[SchoolPhases] [SP] ON [S].[SchoolPhaseId] = [SP].[Id]
    INNER JOIN [dbo].[SchoolTypes] [ST] ON [S].[SchoolTypeId] = [ST].[Id]
    INNER JOIN [dbo].[GovernanceTypes] [GT] ON [S].[GovernanceTypeId] = [GT].[Id]
    INNER JOIN [dbo].[IntakeTypes] [IT] ON [S].[IntakeTypeId] = [IT].[Id]
    LEFT JOIN [dbo].[PayZones] [PZ] ON [S].[PayZoneId] = [PZ].[Id]
    LEFT JOIN [dbo].[SpecialSchoolOrganisations] [SSO] ON [S].[SpecialSchoolOrganisationId] = [SSO].[Id]
    LEFT JOIN [dbo].[SpecialSchoolTypes] [SST] ON [S].[SpecialSchoolTypeId] = [SST].[Id]
    CROSS APPLY [dbo].[fn_person_get_name]([S].[HeadTeacherId], 2, 0 ,1) HT

WHERE [S].[Id] = @schoolId
END