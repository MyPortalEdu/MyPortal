using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IPersonDietaryRequirementRepository : IEntityRepository<PersonDietaryRequirement>
{
    Task<IEnumerable<PersonDietaryRequirement>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
