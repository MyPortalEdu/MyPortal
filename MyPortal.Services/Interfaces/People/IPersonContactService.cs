using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Person-level contact methods (owned emails + phone numbers) keyed by <c>personId</c>.
/// Subtype-agnostic and <em>unauthorized</em>: callers (e.g. <see cref="IStaffContactService"/>)
/// must resolve the person and enforce their own access rules first. This is the shared core that
/// staff / students / contacts delegate to so the reconcile logic lives in exactly one place.
/// </summary>
public interface IPersonContactService
{
    Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid personId,
        CancellationToken cancellationToken);

    /// <summary>Whole-collection replace: inserts new rows, updates matched ids, soft-deletes the rest.</summary>
    Task UpdateContactDetailsAsync(Guid personId, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
