using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Owns the Emergency Contacts area: a staff member's next-of-kin, each a link to a shared
/// Contact record (a Person facet reused across relationships). HR-maintained — All-scope view
/// and edit only. The save is a whole-area replace, the links reconciled by id.
/// </summary>
public interface IStaffNextOfKinService
{
    Task<StaffNextOfKinAreaResponse> GetNextOfKinAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task UpdateNextOfKinAsync(Guid staffMemberId, StaffNextOfKinAreaUpsertRequest model,
        CancellationToken cancellationToken);
}
