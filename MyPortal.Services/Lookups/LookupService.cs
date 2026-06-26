using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Lookups;

namespace MyPortal.Services.Lookups;

public class LookupService : BaseService, ILookupService
{
    private readonly IAgencyTypeRepository _agencyTypes;
    private readonly IGovernanceTypeRepository _governanceTypes;
    private readonly IIntakeTypeRepository _intakeTypes;
    private readonly ISchoolPhaseRepository _schoolPhases;
    private readonly ISchoolTypeRepository _schoolTypes;

    public LookupService(IAuthorizationService authorizationService, ILogger<BaseService> logger,
        IAgencyTypeRepository agencyTypes, IGovernanceTypeRepository governanceTypes,
        IIntakeTypeRepository intakeTypes, ISchoolPhaseRepository schoolPhases, ISchoolTypeRepository schoolTypes)
        : base(authorizationService, logger)
    {
        _agencyTypes = agencyTypes;
        _governanceTypes = governanceTypes;
        _intakeTypes = intakeTypes;
        _schoolPhases = schoolPhases;
        _schoolTypes = schoolTypes;
    }

    public async Task<IList<LookupResponse>> GetAgencyTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await _agencyTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToAlphabeticalLookup();
    }

    public async Task<IList<LookupResponse>> GetGovernanceTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await _governanceTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetIntakeTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await _intakeTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetSchoolPhasesAsync(CancellationToken cancellationToken)
    {
        var rows = await _schoolPhases.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }

    public async Task<IList<LookupResponse>> GetSchoolTypesAsync(CancellationToken cancellationToken)
    {
        var rows = await _schoolTypes.GetListAsync(cancellationToken: cancellationToken);
        return rows.ToOrderedLookup();
    }
}
