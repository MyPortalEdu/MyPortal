using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Staff-flavoured Equality &amp; Diversity service: enforces staff access under
/// <see cref="StaffArea.EqualityDetails"/> (HR-only edit, self/HR view — no Managed scope),
/// resolves the staff member to a person, delegates the person-level fields to
/// <see cref="IPersonEqualityService"/>, and owns the staff disability declaration/specifics.
/// </summary>
public interface IStaffEqualityService
{
    Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateEqualityDetailsAsync(Guid staffMemberId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
