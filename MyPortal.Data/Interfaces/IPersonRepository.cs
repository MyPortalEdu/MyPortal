using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

/// <summary>
/// Repository for the shared <c>Person</c> record. Only the entity-CRUD surface from
/// <see cref="IEntityRepository{Person}"/> is exposed — reads of person data are always
/// shaped by the subtype context (staff profile, student profile, etc.) and live on the
/// subtype repository alongside their joins.
/// </summary>
public interface IPersonRepository : IEntityRepository<Person>
{
    Task<IReadOnlyList<PersonSearchResponse>> SearchAsync(string like, CancellationToken cancellationToken);
}
