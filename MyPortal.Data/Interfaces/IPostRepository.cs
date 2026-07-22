using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IPostRepository : IEntityRepository<Post>
{
    Task<IEnumerable<PostRow>> GetAllWithUsageAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task<bool> ReferenceExistsAsync(string reference, Guid? excludePostId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
