using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.Services.Lookups;

public class LookupService(
    IAuthorizationService authorizationService,
    ILogger<BaseService> logger,
    IAgencyTypeRepository agencyTypes,
    IGovernanceTypeRepository governanceTypes,
    IIntakeTypeRepository intakeTypes,
    ISchoolPhaseRepository schoolPhases,
    ISchoolTypeRepository schoolTypes,
    IPayZoneRepository payZones,
    ISpecialSchoolOrganisationRepository specialSchoolOrganisations,
    ISpecialSchoolTypeRepository specialSchoolTypes)
    : BaseService(authorizationService, logger), ILookupService
{
    public async Task<IList<LookupResponse>> GetAgencyTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await agencyTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToAlphabeticalLookup();
    }

    public async Task<IList<LookupResponse>> GetGovernanceTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await governanceTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetIntakeTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await intakeTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetSchoolPhasesAsync(CancellationToken cancellationToken)
    {
        var rows = await schoolPhases.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetSchoolTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await schoolTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetPayZonesAsync(CancellationToken cancellationToken)
    {
        var rows = await payZones.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToAlphabeticalLookup();
    }

    public async Task<IList<LookupResponse>> GetSpecialSchoolOrganisationsAsync(CancellationToken cancellationToken)
    {
        var rows = await specialSchoolOrganisations.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetSpecialSchoolTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await specialSchoolTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }
}
