using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentPepContributorRepository : IEntityRepository<StudentPepContributor>
{
    Task<IEnumerable<PepContributorResponse>> GetByPepIdAsync(Guid studentPepId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
