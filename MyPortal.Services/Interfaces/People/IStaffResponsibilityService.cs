using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Owns the Responsibilities area — a staff member's designated duties (DSL, First Aider, SENCO, …).
/// Read gated on self / line-manager / HR view; write is HR-only (All scope).
/// </summary>
public interface IStaffResponsibilityService
{
    Task<StaffResponsibilitiesResponse> GetResponsibilitiesAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateResponsibilitiesAsync(Guid staffMemberId, StaffResponsibilitiesUpsertRequest model,
        CancellationToken cancellationToken);
}
