using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Shared person-level equality mechanics (ethnicity, nationality, first language, marital
/// status, religion, sexual orientation, gender identity). Keyed by personId and deliberately
/// auth-free — the subtype service that calls in owns access control. The staff-level disability
/// declaration is handled by the staff service, not here.
/// </summary>
public interface IPersonEqualityService
{
    /// <summary>
    /// Reads the person-level equality fields and populates them — plus their option lists — onto
    /// a <see cref="StaffEqualityDetailsResponse"/>. The disability fields are left for the caller.
    /// </summary>
    Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid personId, CancellationToken cancellationToken);

    /// <summary>Writes the person-level equality FKs only (ignores disability fields on the model).</summary>
    Task UpdateEqualityDetailsAsync(Guid personId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken, IUnitOfWork? uow = null);
}
