using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Contact Details area of the staff profile: a staff member's owned emails and phone numbers.
/// Gated as part of <see cref="StaffArea.BasicDetails"/> (which the access model defines as
/// covering contact methods). Addresses are shared and arrive in a later slice.
/// </summary>
public interface IStaffContactService
{
    Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken);

    Task UpdateContactDetailsAsync(Guid staffMemberId, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
