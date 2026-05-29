using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Infrastructure for the shared <c>Person</c> record, composed by the subtype services
/// (staff/student/contact/agent). It performs NO authorization — the calling layer owns the
/// permission gate. Every mutating method accepts an optional <see cref="IUnitOfWork"/> so the
/// caller can fold the person write into its own transaction.
///
/// Read methods are intentionally NOT exposed here: every read of person data is shaped by the
/// subtype context (e.g. a staff profile header bundles person bio with staff fields), so
/// reads live on the subtype service and gate per-subtype.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Creates a Person (and its backing directory). When <paramref name="uow"/> is supplied the
    /// work joins the caller's transaction and the caller owns the commit.
    /// </summary>
    Task<Guid> CreateAsync(PersonUpsertRequest model, CancellationToken cancellationToken, IUnitOfWork? uow = null);

    /// <summary>Updates a Person's biographical fields. See <see cref="CreateAsync"/> for the
    /// <paramref name="uow"/> transaction semantics.</summary>
    Task UpdateAsync(Guid id, PersonUpsertRequest model, CancellationToken cancellationToken, IUnitOfWork? uow = null);

    /// <summary>Hard-deletes a Person and its backing directory. See <see cref="CreateAsync"/>
    /// for the <paramref name="uow"/> transaction semantics.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null);
}
