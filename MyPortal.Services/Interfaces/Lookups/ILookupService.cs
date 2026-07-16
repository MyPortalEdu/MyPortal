using MyPortal.Contracts.Models;

namespace MyPortal.Services.Interfaces.Lookups;

/// <summary>
/// Read-only access to the simple <c>LookupEntity</c>-backed reference catalogues
/// (Description / Active rows). Drives dropdowns on edit screens — one method per
/// table so each call returns the slice the caller actually needs and the
/// permission gate can be applied per-controller.
/// </summary>
public interface ILookupService
{
    Task<IList<LookupResponse>> GetAgencyTypesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetGovernanceTypesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetIntakeTypesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetSchoolPhasesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetSchoolTypesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetPayZonesAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetSpecialSchoolOrganisationsAsync(CancellationToken cancellationToken);
    Task<IList<LookupResponse>> GetSpecialSchoolTypesAsync(CancellationToken cancellationToken);
}
