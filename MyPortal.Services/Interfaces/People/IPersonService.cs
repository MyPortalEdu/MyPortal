using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Basic biographical fields of a person — the subset that any subtype's "basic details" area
/// composes into. Excludes equality fields (NHS no., ethnicity, nationality, first language,
/// marital status, religion, sexual orientation, gender identity — all owned by the Equality
/// area) and any subtype-specific fields (Code, Bank, etc.). Passed to
/// <see cref="IPersonService.UpdateBasicBioAsync"/>.
/// </summary>
public sealed record PersonBasicBio(
    string? Title,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? PreferredFirstName,
    string? PreferredLastName,
    Guid? PhotoId,
    string Gender,
    DateTime? Dob,
    DateTime? Deceased);

/// <summary>
/// Infrastructure for the shared <c>Person</c> record, composed by the subtype services
/// (staff/student/contact/agent). It performs NO authorization — the calling layer owns the
/// permission gate. Every mutating method accepts an optional <see cref="IUnitOfWork"/> so the
/// caller can fold the person write into its own transaction.
///
/// Both Create and UpdateBasicBio take the shared <see cref="PersonBasicBio"/>, so equality
/// fields (NhsNumber, EthnicityId) are never set through here — they're a separate per-area
/// concern. This mirrors the per-area edit model: each surface only writes the fields it gates.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Creates a Person (and its backing directory) populated with basic bio only. Equality
    /// fields are left null and populated post-creation via the equality-area endpoint.
    /// </summary>
    Task<Guid> CreateAsync(PersonBasicBio bio, CancellationToken cancellationToken, IUnitOfWork? uow = null);

    /// <summary>Updates the basic bio fields only. Equality-sensitive fields are untouched —
    /// they have their own per-area update method (to land with the EqualityDetails area).</summary>
    Task UpdateBasicBioAsync(Guid personId, PersonBasicBio bio, CancellationToken cancellationToken,
        IUnitOfWork? uow = null);

    /// <summary>Hard-deletes a Person and its backing directory.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null);

    /// <summary>
    /// Searches People by name for linking to a user account. Returns empty for terms shorter than
    /// 2 characters. Authorization is the caller's responsibility (this service performs none).
    /// </summary>
    Task<IReadOnlyList<PersonSearchResponse>> SearchAsync(string query, CancellationToken cancellationToken);
}
